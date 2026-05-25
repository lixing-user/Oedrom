using UnityEngine;
using Zenject;

// 地形效果子系統介面（僅限 Terrain 版本）
public interface ITerrainMoveRule
{
    bool AllowsMove(TerrainType terrain, ChessPieceType pieceType);
    bool BlocksMove(TerrainType terrain, ChessPieceType pieceType);
    float GetMovementCost(TerrainType terrain, ChessPieceType pieceType);
}


public class TerrainMoveRule : ITerrainMoveRule
{
    public bool AllowsMove(TerrainType terrain, ChessPieceType pieceType)
    {
        switch (pieceType)
        {
            case ChessPieceType.Knight:
                // 騎士：可以停留在山岳以外的格子
                return terrain != TerrainType.Mountain;

            case ChessPieceType.Rook:
                // 城堡：無法停留在河流(Water)與山岳(Mountain)
                return terrain != TerrainType.Water && terrain != TerrainType.Mountain;

            case ChessPieceType.Pawn:
            case ChessPieceType.Bishop:
            case ChessPieceType.Queen:
            case ChessPieceType.King:
            default:
                // 士兵、主教、皇后、國王：可以停留在所有格子
                return true;
        }
    }

    // 是否阻擋直線移動的棋子
    public bool BlocksMove(TerrainType terrain, ChessPieceType pieceType)
    {
        // 根據你的規則實作「穿越」判斷
        switch (pieceType)
        {
            case ChessPieceType.Queen:
            case ChessPieceType.King:
            case ChessPieceType.Knight:
                // 皇后、國王、騎士：可以穿越所有地形 (不被任何地形阻擋)
                return false;

            case ChessPieceType.Pawn:
            case ChessPieceType.Bishop:
            case ChessPieceType.Rook:
            default:
                // 士兵、主教、城堡：無法穿越河流與山岳 (會被擋住)
                return terrain == TerrainType.Water || terrain == TerrainType.Mountain;
        }
    }

    public float GetMovementCost(TerrainType terrain, ChessPieceType pieceType)
    {
        // 這裡可以寫得很靈活！例如：如果棋子是飛行單位(Dragon)，消耗永遠是 1f

        return terrain switch
        {
            TerrainType.Plain => 1.0f,  // 基礎消耗不變 ( * 1.0)
            TerrainType.Forest => 0.8f, // 森林消耗降低 ( * 0.8)
            TerrainType.Water => 1.5f,  // 涉水增加 50% 消耗 ( * 1.5)
            TerrainType.Mountain => 2.0f,// 爬山消耗翻倍 ( * 2.0)
            _ => float.MaxValue
        };
    }
}
