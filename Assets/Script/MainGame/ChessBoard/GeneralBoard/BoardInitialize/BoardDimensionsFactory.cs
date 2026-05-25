using UnityEngine;

public class BoardDimensionsFactory
{
    private readonly BoardSettings _settings;

    public BoardDimensionsFactory(BoardSettings settings)
    {
        _settings = settings;
    }

    public BoardDimensions Create()
    {
        return new BoardDimensions(
            _settings.BoardSizeX,
            _settings.BoardSizeY,
            _settings.TileSize
        );
    }
}
