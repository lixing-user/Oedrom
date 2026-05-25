using System;
using UnityEngine;
using Zenject;

public class BattleResetCoordinator : IInitializable, IDisposable
{
    [Inject] private ITutorialOverrideService _overrideService; // 用來判斷是否在教學/強制勝利模式

    [Inject] private SignalBus _signalBus;
    [Inject] private GameFlowStateMachine _flowMachine;

    [Inject] private BattleContextHolder _battleContext;
    [Inject] private IBattleStateRestorer _battleRestorer;

    [Inject] private IGameBoard _gameBoard;
    [Inject] private IPieceSpawner _pieceSpawner;


    public void Initialize()
    {
        // 1. 監聽教學系統發來的重置請求 (被動執行)
        _signalBus.Subscribe<ForceBattleResetSignal>(ExecuteReset);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<ForceBattleResetSignal>(ExecuteReset);
    }

    // === 處刑邏輯：執行真實的時光倒流 ===
    private void ExecuteReset(ForceBattleResetSignal signal)
    {
        Debug.Log($"[BattleReset] 觸發重置機制，原因：{signal.Reason}");

        // 1. 阻斷玩家輸入
        _flowMachine.GetFlowFacade().DisableAllInput();

        // 2. 清理 Domain 與 View (我們在前段討論的邏輯)
        _pieceSpawner.RemoveAllPieces();
        _gameBoard.ClearBoard();

        // 3. 調用策略介面進行還原 (取代原本寫死的 InitialHandSnapshot 邏輯)
        _battleContext.ResetBattleResult();
        _battleRestorer.RestoreToInitialState();

        // 4. 強制狀態機回到 Setup (不論現在是在 PlayerTurn 還是 GameEnd)
        _flowMachine.ChangeState(GameFlow.Setup);
    }
}


public struct ForceBattleResetSignal
{
    public string Reason { get; }
    public ForceBattleResetSignal(string reason = "") { Reason = reason; }
}

public struct BattleResetCompletedSignal { }

