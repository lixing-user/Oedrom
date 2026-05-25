using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class FairyStockfishManager : IDisposable
{
    private Process process;
    private StreamWriter inputWriter;
    private StreamReader outputReader;

    private string currentVariant = "chess";
    public bool IsEngineReady { get; private set; } = false;
    private bool isDisposing = false;

    // 使用執行緒安全的佇列，確保不會漏接引擎任何一行輸出
    private ConcurrentQueue<string> engineOutputQueue = new ConcurrentQueue<string>();

    public FairyStockfishManager(string _currentVariant)
    {
        currentVariant = _currentVariant;
    }

    public async Task InitializeAsync()
    {
        // 1. 確保變體設定檔已存在於引擎有權讀取的路徑
        EnsureVariantsFileExists();

        // 2. 啟動引擎
        await StartEngineAsync();

        if (!IsEngineReady)
        {
            UnityEngine.Debug.LogError("[Manager] 引擎未能正確啟動，終止初始化。");
            return;
        }

        // 3. 設定引擎的變體檔案路徑 (雙重保險，避免啟動參數漏讀)
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string path = Path.Combine(Application.persistentDataPath, "variants.ini");
        
        // 確保路徑若含有空格，必須用雙引號包覆
        SendCommand($"setoption name VariantPath value {path}");
        
        // 重新發送 uci 指令，讓引擎重新掃描該路徑並更新支援清單
        SendCommand("uci");
        await WaitForEngineResponseAsync("uciok");
#endif

        // 4. 設定具體的變體 (如 my6x6chess)
        await SetVariantAsync(currentVariant);
    }

    #region 檔案與引擎初始化
    private void EnsureVariantsFileExists()
    {
        string targetPath = Path.Combine(Application.persistentDataPath, "variants.ini");
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "variants.ini");

        try
        {
            // 在 Windows 與 iOS，StreamingAssets 的檔案一般可直接透過 File 讀取
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, targetPath, true);
                UnityEngine.Debug.Log($"[Manager] variants.ini 已同步至: {targetPath}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[Manager] 找不到來源設定檔: {sourcePath}");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[Manager] 複製 variants.ini 失敗: {ex.Message}");
        }
    }

    private async Task StartEngineAsync()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string enginePath = Path.Combine(Application.streamingAssetsPath, "fairy-stockfish.exe");
        string variantsIniPath = Path.Combine(Application.persistentDataPath, "variants.ini");

        if (!File.Exists(enginePath))
        {
            UnityEngine.Debug.LogError($"[Manager] 找不到引擎執行檔: {enginePath}");
            return;
        }

        process = new Process();
        process.StartInfo.FileName = enginePath;
        // 啟動參數：嘗試在啟動時直接 load。注意引號防護空白路徑
        process.StartInfo.Arguments = $"load \"{variantsIniPath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();
            inputWriter = process.StandardInput;
            outputReader = process.StandardOutput;

            // 開啟背景執行緒持續讀取引擎輸出
            _ = Task.Run(ReadOutputLoopAsync);

            // 初始 UCI 握手協議
            SendCommand("uci");
            await WaitForEngineResponseAsync("uciok");

            SendCommand("isready");
            await WaitForEngineResponseAsync("readyok");
            
            IsEngineReady = true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[Manager] 啟動 Windows 引擎失敗: {ex.Message}");
        }

#elif UNITY_IOS
        // iOS 系統禁止使用 Process 啟動外部執行檔 (.exe 或 .bin)
        // 未來需替換為 [DllImport("__Internal")] 的 Native Plugin 呼叫方式
        UnityEngine.Debug.LogError("[Manager] iOS 不支援 Process.Start()。請實作 Native Plugin 來呼叫引擎核心。");
        isEngineReady = false;
        await Task.CompletedTask;
#else
        UnityEngine.Debug.LogError("[Manager] 目前的平台尚未實作引擎啟動邏輯。");
        isEngineReady = false;
        await Task.CompletedTask;
#endif
    }
    #endregion

    #region 狀態與參數設定
    public async Task SetVariantAsync(string variantName)
    {
        if (!IsEngineReady) return;

        currentVariant = variantName;

        // 1. 發送切換變體指令
        SendCommand($"setoption name UCI_Variant value {variantName}");

        // 2. 確認引擎已接受並完成切換
        SendCommand("isready");
        await WaitForEngineResponseAsync("readyok");

        // 3. 通知引擎開啟新局 (清空快取)，並等待就緒
        SendCommand("ucinewgame");
        SendCommand("isready");
        await WaitForEngineResponseAsync("readyok");

        UnityEngine.Debug.Log($"[Manager] 變體已成功切換至: {variantName}");

        // 印出棋盤以供在 Console 檢查是否為 6x6 尺寸
        SendCommand("d");
    }

    public void SetEngineParameters(int skillLevel, int aggressiveness)
    {
        SendCommand($"setoption name Skill Level value {skillLevel}");
        SendCommand($"setoption name Aggressiveness value {aggressiveness}");
    }

    public void SetPosition(string fen)
    {
        SendCommand($"position fen {fen}");
    }
    #endregion

    #region 核心通訊 (寫入與讀取)
    private void SendCommand(string command)
    {
        if (process != null && !process.HasExited)
        {
            UnityEngine.Debug.Log($"[To Engine] {command}");
            inputWriter.WriteLine(command);
            inputWriter.Flush();
        }
    }

    private async Task ReadOutputLoopAsync()
    {
        try
        {
            while (process != null && !process.HasExited)
            {
                string line = await outputReader.ReadLineAsync();
                if (line == null)
                {
                    UnityEngine.Debug.LogError("[Manager] 引擎管線已斷開！(引擎發生底層崩潰，請檢查 FEN 或 Variant 規則是否衝突)");
                    IsEngineReady = false;
                    break;
                }

                // 即時印出所有引擎的輸出，方便 Debug (例如觀察 uci 支援的變體清單)
                UnityEngine.Debug.Log($"[Engine Output] {line}");

                // 存入佇列，交給 WaitForEngineResponseAsync 解析
                engineOutputQueue.Enqueue(line);
            }
        }
        catch (Exception ex)
        {
            if (isDisposing)
            {
                UnityEngine.Debug.Log("[Manager] 背景讀取執行緒已安全終止。");
            }
            else
            {
                UnityEngine.Debug.LogError($"[Manager] 讀取引擎輸出時發生例外: {ex.Message}");
            }
        }
    }

    private async Task<string> WaitForEngineResponseAsync(string expectedPrefix, int timeoutMs = 5000)
    {
        int elapsed = 0;
        int interval = 50; // 輪詢間隔 50ms

        while (elapsed < timeoutMs)
        {
            // 處理積壓在佇列中的所有字串
            while (engineOutputQueue.TryDequeue(out string line))
            {
                if (line.StartsWith(expectedPrefix))
                {
                    //engineOutputQueue.Clear(); // 找到目標即清空，避免歷史訊息干擾下次判斷
                    return line;
                }
            }

            await Task.Delay(interval);
            elapsed += interval;
        }

        UnityEngine.Debug.LogWarning($"[Manager] 等待引擎回應超時: {expectedPrefix}");
        return null;
    }

    public async Task<string> GetBestMoveAsync(int thinkTimeMs = 1000)
    {
        SendCommand($"go movetime {thinkTimeMs}");

        string bestMoveLine = await WaitForEngineResponseAsync("bestmove", thinkTimeMs + 2000); // 超時時間設比思考時間略長

        if (!string.IsNullOrEmpty(bestMoveLine))
        {
            string[] parts = bestMoveLine.Split(' ');
            if (parts.Length > 1) return parts[1];
        }

        return null;
    }
    #endregion

    #region 終止與資源釋放
    public void Dispose()
    {
        StopEngine();
    }

    public void StopEngine()
    {
        if (isDisposing) return; // 避免重複關閉
        isDisposing = true;      // 標記為正在銷毀

        if (process != null)
        {
            try
            {
                if (!process.HasExited)
                {
                    inputWriter.WriteLine("quit");
                    process.WaitForExit(1000); // 給予引擎 1 秒正常關閉的時間
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[Manager] 關閉引擎時發生例外: {ex.Message}");
            }
            finally
            {
                process?.Dispose();
                process = null;
            }
        }
    }
    #endregion
}