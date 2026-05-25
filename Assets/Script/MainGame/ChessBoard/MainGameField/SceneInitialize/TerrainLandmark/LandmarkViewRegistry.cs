using System.Collections.Generic;
using UnityEngine;

public interface ILandmarkViewRegistry
{
    void Register(Vector2Int position, LandmarkView view);
    void Unregister(Vector2Int position);
    LandmarkView GetViewAt(Vector2Int position);
}

public class LandmarkViewRegistry : ILandmarkViewRegistry
{
    private readonly Dictionary<Vector2Int, LandmarkView> _views = new Dictionary<Vector2Int, LandmarkView>();

    public void Register(Vector2Int position, LandmarkView view)
    {
        _views[position] = view;
    }

    public void Unregister(Vector2Int position)
    {
        if (_views.ContainsKey(position))
        {
            _views.Remove(position);
        }
    }

    public LandmarkView GetViewAt(Vector2Int position)
    {
        return _views.TryGetValue(position, out var view) ? view : null;
    }
}