using UnityEngine;
using System.Collections;
using Zenject;
using System.Linq;

public class BoardInitializeServiceE : IBoardInitializationService
{
    [Inject] private readonly BoardDimensions _dimensions;
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardQueryService _boardQuery;

    [Inject] private readonly IBoardTileGenerator _tileGenerator; // Infrastructure
    [Inject] private readonly IPieceSpawner _pieceSpawner;
    [Inject] private readonly IShardSpawner _escapePointSpawner;

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

        // 3. 生成逃脫點
        yield return SetupEscapeTarget();

        MarkPlayersEscaper();
    }

    private IEnumerator SpawnAllPieces()
    {
        int boardSize = _dimensions.BoardSizeX;

        #region 測試棋盤（FEN硬編碼）
        foreach (var pieceData in EscapeBattleSetup.TutorialPieces)
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

    private IEnumerator SetupEscapeTarget()
    {
        foreach (var escapShardData in EscapeBattleSetup.EscapePoint)
        {
            SpawnShard(escapShardData.X, escapShardData.Y);
        }

        yield return null;
    }

    private void SpawnShard(int x, int y)
    {
        try
        {
            _escapePointSpawner.SpawnEscapePoint(x, y);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成逃脫點失敗at ({x},{y}): {ex.Message}");
        }
    }

    private void MarkPlayersEscaper()
    {
        if (_boardQuery == null) return;

        var whitePieces = _boardQuery.GetPiecesForTeam(Team.White);
        var playerQueen = whitePieces.FirstOrDefault(p => p.Type == ChessPieceType.Queen);

        if (playerQueen != null)
        {
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = playerQueen.Id,
                IsPrimary = true
            });
            Debug.Log($"[BoardInitializeT] 成功鎖定並標記玩家皇后: {playerQueen.Id}");
        }
        else
        {
            Debug.LogWarning("未找到玩家的皇后，無法標記為逃脫者");
        }
    }
}
