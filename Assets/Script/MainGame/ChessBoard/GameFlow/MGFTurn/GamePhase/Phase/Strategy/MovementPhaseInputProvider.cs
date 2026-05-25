using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface IPhaseInputProvider
{
    // 告訴 Phase：現在是誰在控制？
    Team Team { get; }

    // 獲取移動指令（玩家會等點擊，AI會直接計算）
    Task<List<PendingPieceMove>> GetMovementCommandsAsync();
}

#region 玩家供應器：負責開啟 UI，並等待玩家點擊確認
public class PlayerProvider : IPhaseInputProvider, IInitializable, IDisposable
{
    public Team Team => Team.White;

    [Inject] private SignalBus _signalBus;
    [Inject] private GamePhaseModel _model;
    [Inject] private GamePhaseFacade _facade;

    // 用來將 Event/Signal 轉換為 Async/Await 的橋樑
    private TaskCompletionSource<List<PendingPieceMove>> _tcs;

    public void Initialize()
    {
        _signalBus.Subscribe<PhaseEndRequestedSignal>(OnPhaseEndRequested);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<PhaseEndRequestedSignal>(OnPhaseEndRequested);
    }

    public async Task<List<PendingPieceMove>> GetMovementCommandsAsync()
    {
        _tcs = new TaskCompletionSource<List<PendingPieceMove>>();

        // 1. 開啟玩家專屬的輸入
        if (!_model.HasPlacedCardThisTurn)
        {
            _facade.EnableCardInput();
        }
        _facade.EnableBoardInput();

        Debug.Log("[PlayerProvider] 等待玩家操作與確認...");

        // 2. 等待玩家點擊「結束回合/確認移動」按鈕
        return await _tcs.Task;
    }

    private void OnPhaseEndRequested(PhaseEndRequestedSignal signal)
    {
        if (_tcs != null && !_tcs.Task.IsCompleted)
        {
            Debug.Log($"[PlayerProvider] 玩家確認移動: {signal.RequestSource}");
            // 關閉輸入
            _facade.DisableAllInput();

            // 回傳 Model 中記錄的 PendingMoves 快照
            var snapshot = new List<PendingPieceMove>(_model.PendingMoves);
            _tcs.SetResult(snapshot);
        }
    }
}
#endregion

#region AI 供應器：負責向引擎要資料，並轉為移動清單
public class AIProvider : IPhaseInputProvider
{
    public Team Team => Team.Black;
    [Inject] private IAIService _aiService;
    [Inject] private IBoardQueryService _boardQuery;

    public async Task<List<PendingPieceMove>> GetMovementCommandsAsync()
    {
        Debug.Log("[AIProvider] AI 開始運算...");

        // 1. 等待 AI 引擎給出答案
        AIMovement result = await _aiService.ExecuteMoveAsync();
        var piece = _boardQuery.GetPieceById(result.PieceId);

        if (!result.IsSuccess)
        {
            Debug.LogError($"[AIProvider] AI 行動失敗: {result.ErrorMessage}");
            return new List<PendingPieceMove>(); // 回傳空清單，代表不移動
        }

        Debug.Log($"[AIProvider] AI 決定移動: 棋子 {result.PieceId} 到 {result.To}");

        // 2. 轉換為遊戲的指令格式
        var pendingMove = new PendingPieceMove
        {
            PieceId = result.PieceId,
            From = new Vector2Int(piece.CurrentPosition.X, piece.CurrentPosition.Y),
            To = new Vector2Int(result.To.X, result.To.Y)
        };

        return new List<PendingPieceMove> { pendingMove };
    }
}
#endregion