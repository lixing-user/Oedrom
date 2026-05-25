using System;
using UnityEngine;

public class BoardDimensions 
{
    public int BoardSizeX { get; }
    public int BoardSizeY { get; }
    public float TileSize { get; }


    public BoardDimensions(int sizeX, int sizeY, float tileSize)
    {
        if (sizeX <= 0 || sizeY <= 0)
            throw new ArgumentException("棋盤尺寸必須大於0");

        BoardSizeX = sizeX;
        BoardSizeY = sizeY;
        TileSize = tileSize;
    }

    public int TotalTiles => BoardSizeX * BoardSizeY;

    public Vector3 GridBounds => new Vector3(
    (BoardSizeX / 2f) * TileSize,
    0,
    (BoardSizeY / 2f) * TileSize
);
}
