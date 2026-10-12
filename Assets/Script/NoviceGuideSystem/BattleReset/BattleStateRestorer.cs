using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

// Application/Battle/IBattleStateRestorer.cs
public interface IBattleStateRestorer
{
    // 首次進入戰鬥時呼叫，用來備份初始狀態
    void CaptureInitialStateIfNeeded();

    // 發生失敗重置時呼叫，用來重建初始狀態
    IEnumerator RestoreToInitialState();
}

#region Normal Battle
public class NormalBattleRestorer : IBattleStateRestorer, IInitializable, IDisposable
{
    [Inject] private SignalBus _signalBus;

    [Inject] private IHandService _handService;
    [Inject] private IDeckService _deckService;

    [Inject] private IBoardQueryService _boardQuery;
    [Inject] private IPieceSpawner _pieceSpawner;
    [Inject] private BattleContextHolder _battleContext;


    public void Initialize()
    {
        _signalBus.Subscribe<BoardInitializedSignal>(CaptureInitialStateIfNeeded);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<BoardInitializedSignal>(CaptureInitialStateIfNeeded);
    }

    public void CaptureInitialStateIfNeeded()
    {
        var currentBattle = _battleContext.CurrentBattle;
        if (currentBattle == null || currentBattle.InitialBoardSnapshot != null)
            return; // 已經拍過快照，直接略過

        Debug.Log("[NormalBattleRestorer] 紀錄 NormalBattle 動態初始快照");

        // 1. 紀錄手牌
        currentBattle.InitialHandSnapshot = _handService.CreateHandSnapshot();

        // 2. 紀錄棋盤實體
        var allPieces = _boardQuery.GetAllPieces().ToList();
        currentBattle.InitialBoardSnapshot = allPieces.Select(p => new PieceDTO(p)).ToList();
    }

    public IEnumerator RestoreToInitialState()
    {
        var currentBattle = _battleContext.CurrentBattle;
        if (currentBattle == null) yield break;

        Debug.Log("[NormalBattleRestorer] 執行 NormalBattle 狀態還原");

        // 1. 根據快照重建棋盤實體
        foreach (var dto in currentBattle.InitialBoardSnapshot)
        {
            PieceID pieceId = PieceID.FromString(dto.Pieceid);
            PiecePosition pos = new PiecePosition(dto.currentX, dto.currentY);
            Team team = (Team)dto.team;
            _pieceSpawner.SpawnPieceWithId(pieceId, dto.type, pos, team);
        }

        // 2. 還原手牌邏輯與視圖
        if (currentBattle.InitialHandSnapshot != null)
        {
            _handService.RestoreFromSnapshot(currentBattle.InitialHandSnapshot);
            _deckService.ReloadCardViews();
        }

        // 3. 重新標記戰鬥目標
        _signalBus.Fire(new MarkTargetPieceSignal
        {
            TargetPieceId = currentBattle.DefenderId,
            IsPrimary = true
        });

        _signalBus.Fire(new MarkTargetPieceSignal
        {
            TargetPieceId = currentBattle.AttackerId,
            IsPrimary = false
        });

        Debug.Log("[NormalBattleRestorer] 棋盤還原完畢，發送 BattleResetCompletedSignal");
        // 4. 發送重置完成的信號，讓 TutorialStepRunner 知道可以重新開始當前的 Step
        _signalBus.Fire<BattleResetCompletedSignal>();
        yield break;
    }
}
#endregion

#region Chase / Escape Battle
public class StaticBattleRestorer : IBattleStateRestorer
{
    [Inject] private IBoardInitializationService _initService; // 注入對應場景的初始化服務
    [Inject] private CoroutineService _coroutineService;       // 用於執行非同步初始化
    [Inject] private SignalBus _signalBus;

    public void CaptureInitialStateIfNeeded()
    {
        // 什麼都不用做！因為 Chase 和 Escape 的資料本來就是從 TutorialSetup 靜態讀取的。
        Debug.Log("[StaticBattleRestorer] 靜態關卡，無須拍攝快照。");
    }

    public IEnumerator RestoreToInitialState()
    {
        Debug.Log("[StaticBattleRestorer] 執行特殊戰鬥狀態還原，重新呼叫初始化腳本...");

        // 1. 等待棋子與逃脫點真正生成完畢
        yield return _initService.RestoreEntitiesAsync();

        // 2. 發送重置完成的信號，讓 TutorialStepRunner 知道可以重新開始當前的 Step
        Debug.Log("[StaticBattleRestorer] 棋盤還原完畢，發送 BattleResetCompletedSignal");
        _signalBus.Fire<BattleResetCompletedSignal>();
    }
}
#endregion
