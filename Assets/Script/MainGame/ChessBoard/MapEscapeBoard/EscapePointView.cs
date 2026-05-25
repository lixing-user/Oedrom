using UnityEngine;
using Zenject;

public class EscapePointView : MonoBehaviour
{
    public PiecePosition Position { get; private set; }

    [Inject]
    public void Construct(PiecePosition position, Vector3 worldPosition)
    {
        Position = position;  // 接收已驗證的 Domain 座標

        transform.position = worldPosition;
        gameObject.name = $"EscapePointShard_{position.X}_{position.Y}";
    }

    public class Factory : PlaceholderFactory<PiecePosition, Vector3, EscapePointView> { }
}
