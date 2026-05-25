using System;
using System.Linq;
using UnityEngine;
using Zenject;

public enum StandbySource
{
    Spawned,    // 教學系統憑空生成
    Captured    // 戰鬥中捕獲
}

public class StandbyZoneService : IInitializable, IDisposable
{
    [Inject] private readonly ITutorialOverrideService _overrideService;

    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IStandbyZone _standbyZone;
    [Inject] private readonly StandbyZoneView _view;
    [Inject] private readonly IPieceViewRegistry _viewRegistry;

    [Inject] private readonly StandbyLayoutCalculator _calculator;
    [Inject] private readonly IBoardTileGenerator _tileGenerator;
    [Inject] private readonly ITerrainHeightSampler _heightSampler;

    #region Initialization & Disposal
    public void Initialize()
    {
        _signalBus.Subscribe<StandbyZoneUpdatedSignal>(OnStandbyZoneUpdated);
        _signalBus.Subscribe<PieceRemovedSignal>(OnPieceRemoved);
        _signalBus.Subscribe<PieceRestoredSignal>(OnPieceRestored);
        _signalBus.Subscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);

        bool isLockedInit = _overrideService.IsFeatureLocked("StandbyZone");
        _view.UpdateVisibility(isLockedInit);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<StandbyZoneUpdatedSignal>(OnStandbyZoneUpdated);
        _signalBus.TryUnsubscribe<PieceRemovedSignal>(OnPieceRemoved);
        _signalBus.TryUnsubscribe<PieceRestoredSignal>(OnPieceRestored);
        _signalBus.TryUnsubscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);
    }
    #endregion

    #region Signal Handlers
    private void OnStandbyZoneUpdated(StandbyZoneUpdatedSignal signal)
    {
        // 1. 查詢領域狀態
        var pieces = _standbyZone.GetStandbyPieces(signal.Team).ToList();

        Vector3 anchorPos = _view.GetAnchorPosition(signal.Team);
        StandbyLayoutConfig config = _view.Config;

        // 2. 決定演出邏輯 (這就是原本缺乏的 Application 判斷層)
        for (int i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];

            // 3. 計算目標座標 (交給專門的計算機)
            Vector3 targetPos = _calculator.CalculateWorldPosition(anchorPos, i, config);

            // 4. 根據「來源」分流，指揮 View 播放不同的視覺效果
            if (piece.Id == signal.PieceId && signal.Source == StandbySource.Spawned)
            {
                // 如果是剛被教學系統生成的，播生成特效
                _view.PlaySpawnEffect(piece.Id, targetPos);
            }
            else
            {
                // 一般捕獲或重新排列，播放平滑移動
                _view.AnimatePieceToPosition(piece.Id, targetPos);
            }
        }
    }

    private void OnPieceRemoved(PieceRemovedSignal signal)
    {
        if (signal.Mode == CaptureMode.ToStandby)
        {
            RefreshLayout(signal.Team, StandbySource.Captured, signal.PieceId);
        }
    }

    private void OnPieceRestored(PieceRestoredSignal signal)
    {
        // 1. 由 Service 負責計算棋盤上的目標世界座標
        Vector3 tileCenter = _tileGenerator.GetTileCenter(signal.Position.X, signal.Position.Y);
        float terrainHeight = _heightSampler.GetTerrainHeightAt(signal.Position.X, signal.Position.Y);
        Vector3 boardWorldPos = new Vector3(tileCenter.x, terrainHeight, tileCenter.z);

        // 2. 指揮 View 執行「飛回棋盤」的動畫
        _view.AnimatePieceToBoard(signal.PieceId, boardWorldPos, () =>
        {
            var view = _viewRegistry.GetView(signal.PieceId);
            view?.SyncLogicalPosition();
        });

        // 棋子離開待命區，傳入 null 讓剩餘棋子平滑補位
        RefreshLayout(signal.Team, StandbySource.Spawned, null);
    }

    private void OnFeatureLockChanged(FeatureLockChangedSignal signal)
    {
        if (signal.FeatureName == DynamicCaptureHandler.FeatureName_StandbyZone)
        {
            _view.UpdateVisibility(signal.IsLocked);
        }
    }


    /// <summary>
    /// 核心分流與排版邏輯
    /// </summary>
    public void RefreshLayout(Team team, StandbySource source, PieceID targetPieceId)
    {
        // 1. 從 Domain 取回最新狀態
        var pieces = _standbyZone.GetStandbyPieces(team).ToList();

        // 2. 向 View 索取基礎設定
        Vector3 anchorPos = _view.GetAnchorPosition(team);
        StandbyLayoutConfig config = _view.Config;

        // 3. 遍歷並計算
        for (int i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];

            // 透過無狀態計算機求得精確座標
            Vector3 targetPos = _calculator.CalculateWorldPosition(anchorPos, i, config);

            _view.SetPieceToStandbyMaterial(piece.Id);

            // 4. 動畫分流邏輯
            // 只要 targetPieceId 不是 null，且確實為本次生成的目標棋子，就播生成特效
            if (targetPieceId != null && piece.Id == targetPieceId && source == StandbySource.Spawned)
            {
                _view.PlaySpawnEffect(piece.Id, targetPos);
            }
            else
            {
                // 其他棋子（包含未帶 ID 的批次更新），一律執行平滑移動排列
                _view.AnimatePieceToPosition(piece.Id, targetPos);
            }
        }
    }
    #endregion
}


public class StandbyZoneUpdatedSignal
{
    public Team Team { get; }
    public StandbySource Source { get; }
    public PieceID PieceId { get; }

    public StandbyZoneUpdatedSignal(Team team, StandbySource source, PieceID pieceId = null)
    {
        Team = team;
        Source = source;
        PieceId = pieceId;
    }
}