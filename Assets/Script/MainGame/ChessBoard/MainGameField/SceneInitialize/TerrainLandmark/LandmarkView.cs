using UnityEngine;
using Zenject;

public class LandmarkView : MonoBehaviour
{
    public LandmarkType Type { get; private set; }
    public PiecePosition Position { get; private set; }


    [Inject]
    public void Construct(LandmarkType type, PiecePosition position, Vector3 worldPosition)
    {
        Type = type;
        Position = position;  // 接收已驗證的 Domain 座標

        transform.position = worldPosition;
        gameObject.name = $"Landmark_{type}_{position.X}_{position.Y}";
    }

    public class Factory : PlaceholderFactory<LandmarkType, PiecePosition, Vector3, LandmarkView> { }
}
