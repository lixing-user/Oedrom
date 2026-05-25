using UnityEngine;
using Zenject;

//  Application ：應用地形效果
public class TerrainEffectHandler : ITerrainEffectApplier
{
    [Inject] private readonly ITerrainRepository _terrainRepo;
    [Inject] private readonly SignalBus _signalBus;

    public void ApplyEffects(ChessPiece piece, PiecePosition position)
    {
        var terrain = _terrainRepo.GetTerrainAt(position);

        switch (terrain)
        {
            case TerrainType.Mountain:
                // 該棋子當回合補給減半
                break;
            case TerrainType.Water:
                // 無效果
                break;
            case TerrainType.Forest:
                // 該棋子當回合補給增加
                break;
            case TerrainType.Plain:
                // 無效果
                break;
            default:
                break;
        }
    }
}

public interface ITerrainEffectApplier
{
    void ApplyEffects(ChessPiece piece, PiecePosition position);
}