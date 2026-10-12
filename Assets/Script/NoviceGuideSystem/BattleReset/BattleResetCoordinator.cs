using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class BattleResetCoordinator : IInitializable, IDisposable
{
    [Inject] private ITutorialOverrideService _overrideService; // 用來判斷是否在教學/強制勝利模式

    [Inject] private SignalBus _signalBus;
    [Inject] private CoroutineService _coroutineService;
    [Inject] private GameFlowStateMachine _flowMachine;

    [Inject] private BattleContextHolder _battleContext;
    [Inject] private IBattleStateRestorer _battleRestorer;
    [Inject] private IRewindVisualPresenter _visualPresenter;

    [Inject] private IGameBoard _gameBoard;
    [Inject] private IPieceSpawner _pieceSpawner;

    private bool _isResetting = false;
    private bool _isRestoreCompleted = false;

    public void Initialize()
    {
        // 1. 監聽教學系統發來的重置請求 (被動執行)
        _signalBus.Subscribe<ForceBattleResetSignal>(ExecuteReset);
        _signalBus.Subscribe<BattleResetCompletedSignal>(OnRestoreCompleted);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<ForceBattleResetSignal>(ExecuteReset);
        _signalBus.Unsubscribe<BattleResetCompletedSignal>(OnRestoreCompleted);
    }

    #region 舊版方案
    // === 處刑邏輯：執行真實的時光倒流 ===
    //private void ExecuteReset(ForceBattleResetSignal signal)
    //{
    //    Debug.Log($"[BattleReset] 觸發重置機制，原因：{signal.Reason}");

    //    // 1. 阻斷玩家輸入
    //    _flowMachine.GetFlowFacade().DisableAllInput();

    //    // 2. 清理 Domain 與 View (我們在前段討論的邏輯)
    //    _pieceSpawner.RemoveAllPieces();
    //    _gameBoard.ClearBoard();

    //    // 3. 調用策略介面進行還原 (取代原本寫死的 InitialHandSnapshot 邏輯)
    //    _battleContext.ResetBattleResult();
    //    _battleRestorer.RestoreToInitialState();

    //    // 4. 強制狀態機回到 Setup (不論現在是在 PlayerTurn 還是 GameEnd)
    //    _flowMachine.ChangeState(GameFlow.Setup);
    //}
    #endregion

    private void ExecuteReset(ForceBattleResetSignal signal)
    {
        if (_isResetting) return;
        _isResetting = true;

        Debug.Log($"[BattleReset] 觸發重置機制，原因：{signal.Reason}");

        _coroutineService.StartCoroutine(ResetSequence(signal.Reason));
    }

    private void OnRestoreCompleted() => _isRestoreCompleted = true;

    private IEnumerator ResetSequence(string reason)
    {
        Debug.Log($"[BattleReset] 觸發重置機制，原因：{reason}");
        _flowMachine.GetFlowFacade().DisableAllInput();

        // 1. 視覺層：蓋幕 (Coordinator 掌控等待時間)
        yield return _visualPresenter.PlayCoverIn();

        // 2. 邏輯層：趁黑幕清理 Domain 與 View
        _pieceSpawner.RemoveAllPieces();
        _gameBoard.ClearBoard();
        _battleContext.ResetBattleResult();

        _isRestoreCompleted = false;
        yield return _battleRestorer.RestoreToInitialState();
        _flowMachine.ChangeState(GameFlow.Setup);

        // 3. 邏輯層：呼叫還原並等待完成
        yield return new WaitUntil(() => _isRestoreCompleted);

        // 4. 視覺層：撤幕
        yield return _visualPresenter.PlayCoverOut();

        // 5. 通知系統重置徹底結束 (可供 TutorialStepRunner 重新啟動)
        _signalBus.Fire<RewindSequenceFinishedSignal>();

        _isResetting = false;
    }
}


public struct ForceBattleResetSignal
{
    public string Reason { get; }
    public ForceBattleResetSignal(string reason = "") { Reason = reason; }
}

public struct BattleResetCompletedSignal { }

