using System;
using UnityEngine;

public class BoardDimensions 
{
    public int BoardSizeX { get; }
    public int BoardSizeY { get; }
    public float GridSpacing { get; }


    public BoardDimensions(int sizeX, int sizeY, float gridSpacing)
    {
        if (sizeX <= 0 || sizeY <= 0)
            throw new ArgumentException("棋盤尺寸必須大於0");

        BoardSizeX = sizeX;
        BoardSizeY = sizeY;
        GridSpacing = gridSpacing;
    }

    public int TotalTiles => BoardSizeX * BoardSizeY;

    public Vector3 GridBounds => new Vector3(
    (BoardSizeX / 2f) * GridSpacing,
    0,
    (BoardSizeY / 2f) * GridSpacing
);
}
