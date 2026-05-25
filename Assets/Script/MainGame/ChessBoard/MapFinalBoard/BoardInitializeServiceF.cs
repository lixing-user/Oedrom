using System.Collections;
using UnityEngine;
using Zenject;

public class BoardInitializeServiceF : IBoardInitializationService
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly BoardDimensions _dimensions; // Domain
    [Inject] private readonly BattleContextHolder _battleContext;
    [Inject] private readonly IBoardQueryService _boardQuery;

    [Inject] private readonly IBoardTileGenerator _tileGenerator; // Infrastructure
    [Inject] private readonly IPieceSpawner _pieceSpawner;

    public IEnumerator InitializeBoardAsync()
    {
        // 1. 生成棋盤格子
        _tileGenerator.GenerateAllTiles(
            _dimensions.TileSize,
            _dimensions.BoardSizeX,
            _dimensions.BoardSizeY
        );

        yield return RestoreEntitiesAsync();

        Debug.Log("棋盤初始化完成");
    }

    public IEnumerator RestoreEntitiesAsync()
    {
        // 2. 生成棋子
        yield return SpawnAllPieces();

        MarkBattleTargets();

        // 3. 發送訊號通知 UI 顯示雙方手牌（如果有的話）
        _signalBus.Fire(new ToggleHandDeckSignal());

    }

    private IEnumerator SpawnAllPieces()
    {
        int boardSize = _dimensions.BoardSizeX;
        var battle = _battleContext.CurrentBattle;

        if (battle != null)
        {
            // 固定位置
            SpawnPieceWithId(battle.AttackerId, battle.AttackerType, 4, 0, battle.AttackerTeam);
            SpawnPieceWithId(battle.DefenderId, battle.DefenderType, 4, 7, battle.DefenderTeam);

            #region 正式棋盤（FEN解析）
            foreach (var pieceData in FinalBattleTutorialSetup.TutorialPieces)
            {
                SpawnPiece(pieceData.Type, pieceData.X, pieceData.Y, pieceData.Team);
            }
            #endregion
        }
        else
        {
            #region 測試用標準棋盤
            SpawnPiece(ChessPieceType.King, 4, 0, Team.White);
            SpawnPiece(ChessPieceType.King, 4, 7, Team.Black);
            foreach (var pieceData in FinalBattleTutorialSetup.TutorialPieces)
            {
                SpawnPiece(pieceData.Type, pieceData.X, pieceData.Y, pieceData.Team);
            }
            #endregion
        }

        yield return null;
    }

    private void SpawnPieceWithId(PieceID existingId, ChessPieceType type, int x, int y, Team team)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            _pieceSpawner.SpawnPieceWithId(existingId, type, position, team);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成棋子失敗（指定ID）[{type}] at ({x},{y}): {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }

    private void SpawnPiece(ChessPieceType type, int x, int y, Team team)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            _pieceSpawner.SpawnSinglePiece(type, position, team);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成棋子失敗 [{type}] at ({x},{y}): {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }

    private void MarkBattleTargets()
    {
        var battle = _battleContext.CurrentBattle;
        if (battle != null)
        {
            // 標記防守方
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = battle.DefenderId,
                IsPrimary = true
            });

            // 標記攻擊方
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = battle.AttackerId,
                IsPrimary = false
            });

            Debug.Log($"[BoardInitialize] 已標記戰鬥雙方目標");
        }
    }
}
