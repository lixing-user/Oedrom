using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface ITutorialProgressService
{
    // 查詢
    bool IsStepCompleted(string stepId);

    // 寫入
    void MarkStepCompleted(string stepId);
    void ResetProgress();                    // 重置（新遊戲用）
}

public class TutorialProgressService : ITutorialProgressService
{
    #region PlayerPrefs 鍵值定義
    private const string Key_CompletedSteps = "Tutorial_CompletedSteps";
    #endregion

    private readonly SignalBus _signalBus;
    // 記憶體快取：儲存已完成的特定步驟 ID，提升查詢效率
    private HashSet<string> _completedStepsCache;


    public TutorialProgressService(SignalBus signalBus)
    {
        _signalBus = signalBus;
        // 在建構時初始化快取
        InitializeCache();
    }

    private void InitializeCache()
    {
        _completedStepsCache = new HashSet<string>();

        // 從 PlayerPrefs 讀取逗號分隔的字串，例如 "001PlaceKnight,002MovePawn"
        string savedSteps = PlayerPrefs.GetString(Key_CompletedSteps, string.Empty);

        if (!string.IsNullOrEmpty(savedSteps))
        {
            string[] steps = savedSteps.Split(',');
            foreach (var step in steps)
            {
                _completedStepsCache.Add(step);
            }
        }
    }

    #region 查詢功能實作
    public bool IsStepCompleted(string stepId) => _completedStepsCache.Contains(stepId);
    #endregion

    #region 寫入功能實作
    public void MarkStepCompleted(string stepId)
    {
        if (_completedStepsCache.Contains(stepId))
            return; // 已經完成過，不重複寫入

        // 1. 更新快取
        _completedStepsCache.Add(stepId);

        // 2. 核心過濾：找出「不是」以 Intent_ 開頭的標記
        var stepsToSave = _completedStepsCache.Where(id => !id.StartsWith("Intent_"));

        // 3. 序列化 Set 為字串並存檔
        PlayerPrefs.SetString(Key_CompletedSteps, string.Join(",", stepsToSave));

        // 4. 強制寫入磁碟
        PlayerPrefs.Save();

        Debug.Log($"[Tutorial] 準備發射 Signal: {stepId}");
        _signalBus.Fire(new GameFlagAddedSignal { FlagId = stepId });
    }
   

    public void ResetProgress()                    // 重置（新遊戲用）
    {
        PlayerPrefs.DeleteKey(Key_CompletedSteps);
        PlayerPrefs.Save();

        // 清空記憶體快取
        _completedStepsCache.Clear();

        Debug.Log("[TutorialProgress] 新手教學進度已重置 (New Game)");
    }
    #endregion
}

public struct GameFlagAddedSignal
{
    public string FlagId;
}