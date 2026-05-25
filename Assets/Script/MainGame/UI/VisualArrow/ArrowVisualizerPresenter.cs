using System.Collections.Generic;
using UnityEngine;

public interface IArrowVisualizerPresenter
{
    ParabolicArrowView SpawnArrow(Vector3 startWorldPos, Vector3 endWorldPos);
    void ClearAllArrows();
}

public class ArrowVisualizerPresenter : MonoBehaviour, IArrowVisualizerPresenter
{
    [Header("Ωb¿Y≥]©w")]
    [SerializeField] private ParabolicArrowView _arrowPrefab;

    private List<ParabolicArrowView> _activeArrows = new List<ParabolicArrowView>();

    public ParabolicArrowView SpawnArrow(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (_arrowPrefab == null) return null;

        var arrow = Instantiate(_arrowPrefab, startWorldPos, Quaternion.identity);
        arrow.Setup(startWorldPos, endWorldPos);
        _activeArrows.Add(arrow);

        return arrow;
    }

    public void ClearAllArrows()
    {
        foreach (var arrow in _activeArrows)
        {
            if (arrow != null) Destroy(arrow.gameObject);
        }
        _activeArrows.Clear();
    }
}
