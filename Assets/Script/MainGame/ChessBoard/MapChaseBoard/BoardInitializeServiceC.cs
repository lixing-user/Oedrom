using UnityEngine;
using System.Collections;
using Zenject;
using System.Linq;

public class BoardInitializeServiceC : IBoardInitializationService
{
    [Inject] private readonly BoardDimensions _dimensions;
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardQueryService _boardQuery;

    [Inject] private readonly IBoardTileGenerator _tileGenerator; // Infrastructure
    [Inject] private readonly IPieceSpawner _pieceSpawner;

    public IEnumerator InitializeBoardAsync()
    {
        // 1. 生成棋盤格子
        _tileGenerator.GenerateAllTiles(
            _dimensions.GridSpacing,
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

        MarkEnemyKing();
    }

    private IEnumerator SpawnAllPieces()
    {
        int boardSize = _dimensions.BoardSizeX;

        #region 測試棋盤（FEN硬編碼）
        foreach (var pieceData in ChaseBattleSetup.TutorialPieces)
        {
            SpawnPiece(pieceData.Type, pieceData.X, pieceData.Y, pieceData.Team);
        }
        #endregion

        yield return null;
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

    private void MarkEnemyKing()
    {
        if (_boardQuery == null) return;

        var blackPieces = _boardQuery.GetPiecesForTeam(Team.Black);
        var enemyKing = blackPieces.FirstOrDefault(p => p.Type == ChessPieceType.King);

        if (enemyKing != null)
        {
            // 發射訊號標記國王
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = enemyKing.Id,
                IsPrimary = true
            });
            Debug.Log($"[BoardInitializeT] 成功鎖定並標記黑方國王: {enemyKing.Id}");
        }
        else
        {
            Debug.LogWarning("[BoardInitializeT] 找不到黑方國王可以標記！");
        }
    }
}
