using UnityEngine;
using Zenject;

public class BoardTileGenerator : IBoardTileGenerator
{
    [Inject] private TileView.Factory _tileFactory;

    [Inject] private readonly BoardDimensions _dimensions;//Domain 核心邏輯（棋盤格式）
    [Inject(Id = "BoardTransform")] private readonly Transform _boardTransform;

    [Inject] private readonly ITilePositionCalculator _tilePosition;

    private GameObject[,] tiles;


    public void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        Debug.Log($"開始生成棋盤: {tileCountX}x{tileCountY}");

        tiles = new GameObject[_dimensions.BoardSizeX, _dimensions.BoardSizeY];

        for (int x = 0; x < _dimensions.BoardSizeX; x++)
            for (int y = 0; y < _dimensions.BoardSizeY; y++)
            {
                var position = new TilePosition(x, y);

                Vector3 localPosition = _tilePosition.CalculateTileLocalCenter(x, y);

                var tileView = _tileFactory.Create(position, localPosition);

                tileView.transform.SetParent(_boardTransform, false);

                tiles[x, y] = tileView.gameObject;
            }
    }

   

    public Vector3 GetTileCenter(int x, int y)
    {
        if (x < 0 || x >= _dimensions.BoardSizeX || y < 0 || y >= _dimensions.BoardSizeY)
        {
            Debug.LogError($"[BoardTileGenerator] 無效座標: ({x}, {y})");
            return Vector3.zero;
        }

        if (tiles != null && tiles[x, y] != null)
        {
            return tiles[x, y].transform.position;
        }

        //回滾舊邏輯
        Vector3 localPos = _tilePosition.CalculateTileLocalCenter(x, y);

        if (_boardTransform != null)
        {
            return _boardTransform.TransformPoint(localPos);
        }

        Debug.LogWarning($"[BoardTileGenerator] Tiles 陣列未初始化，返回理論座標 ({x}, {y})");

        return _boardTransform.TransformPoint(localPos);
    }

    public GameObject GetTileObject(int x, int y)
    {
        if (tiles == null)
        {
            Debug.LogWarning("[BoardTileGenerator] Tiles 陣列未初始化");
            return null;
        }

        if (x < 0 || x >= _dimensions.BoardSizeX || y < 0 || y >= _dimensions.BoardSizeY)
        {
            Debug.LogError($"[BoardTileGenerator] 無效座標: ({x}, {y})");
            return null;
        }

        return tiles[x, y];
    }

    public Vector2Int LookUpTilesIndex(GameObject hitInfo)
    {
        var tileView = hitInfo.GetComponent<TileView>();
        if (tileView != null)
        {
            return new Vector2Int(tileView.Position.X, tileView.Position.Y);
        }

        //回滾舊邏輯
        for (int x = 0; x < _dimensions.BoardSizeX; x++)
            for (int y = 0; y < _dimensions.BoardSizeY; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return -Vector2Int.one;
    }
}

public interface IBoardTileGenerator
{
    void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY);
    Vector3 GetTileCenter(int x, int y);
    Vector2Int LookUpTilesIndex(GameObject hitInfo);
}