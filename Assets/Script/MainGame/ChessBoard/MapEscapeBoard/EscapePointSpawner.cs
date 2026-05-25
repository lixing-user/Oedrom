using UnityEngine;
using Zenject;

public interface IShardSpawner
{
    void SpawnEscapePoint(int x, int y);
}

public class EscapePointSpawner : IShardSpawner
{
    [Inject] private readonly EscapePointView.Factory _factory;
    [Inject] private readonly BoardDimensions _dimensions;
    [Inject] private readonly IBoardTileGenerator _tileGenerator;

    public void SpawnEscapePoint(int x, int y)
    {
        var position = PiecePosition.Create(x, y, _dimensions);
        Vector3 worldPosition = _tileGenerator.GetTileCenter(x, y);

        var landmark = _factory.Create(position, worldPosition);

        Debug.Log($"[EscapePointSpawner] 已生成逃脫點 at ({x},{y})");
    }
}
