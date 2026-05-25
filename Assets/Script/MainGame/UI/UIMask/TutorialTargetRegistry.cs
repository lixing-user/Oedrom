using System.Collections.Generic;
using UnityEngine;

public class TutorialTargetRegistry : MonoBehaviour
{
    [System.Serializable]
    public struct TargetMapping
    {
        public string TargetID;
        public RectTransform TargetRect;
    }

    [Header("在這裡拖入你預先擺好的隱形標靶")]
    public List<TargetMapping> predefinedTargets;

    private Dictionary<string, RectTransform> _targetDict = new();

    private void Awake()
    {
        foreach (var mapping in predefinedTargets)
        {
            _targetDict[mapping.TargetID] = mapping.TargetRect;
        }
    }

    public RectTransform GetTarget(string targetID)
    {
        return _targetDict.TryGetValue(targetID, out var rect) ? rect : null;
    }
}
