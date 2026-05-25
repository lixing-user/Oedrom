using UnityEngine;

public enum ShopItemType { Piece, Supply }

public class ShopItemData
{
    public string Id { get; }
    public ShopItemType ItemType { get; }
    public int Cost { get; }

    public ChessPieceType? PieceType { get; }       // 棋子透過 CardStaticDataRegistry 拿詳細資料
    public int SupplyAmount { get; }


    // 棋子商品建構子
    public ShopItemData(string id, ChessPieceType pieceType, int cost)
    {
        Id = id;
        ItemType = ShopItemType.Piece;
        PieceType = pieceType;
        Cost = cost;
    }

    // 補給商品建構子
    public ShopItemData(string id, int supplyAmount, int cost)
    {
        Id = id;
        ItemType = ShopItemType.Supply;
        SupplyAmount = supplyAmount;
        Cost = cost;
    }
}
