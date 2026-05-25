using UnityEngine;

public class SellTriggerContainer : MonoBehaviour
{
    [SerializeField] private Renderer _areaRenderer;
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private ParticleSystem _triggerEffect;

    public void SetHighlight(bool isHighlight)
    {
        _areaRenderer.material = isHighlight ? _highlightMaterial : _normalMaterial;
    }

    public void PlayTriggerEffect()
    {
        if (_triggerEffect != null) _triggerEffect.Play();
    }
}
