using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class ResourceContainerBase : MonoBehaviour
{
    [SerializeField] protected Image _resourceBar;

    protected IResourceManager _resourceManager;
    protected SignalBus _signalBus;

    protected abstract VariableType ResourceType { get; }
    protected abstract float MaxValue { get; }


    [Inject]
    public void Construct(IResourceManager resourceManager, SignalBus signalBus)
    {
        _resourceManager = resourceManager;
        _signalBus = signalBus;
    }


    protected virtual void OnEnable()
    {
        float currentValue = _resourceManager.GetVariable(ResourceType);
        UpdateResourceBar(currentValue);

        _signalBus.Subscribe<ResourceChangedSignal>(OnResourceChanged);
    }
    protected virtual void OnDisable()
    {
        _signalBus.TryUnsubscribe<ResourceChangedSignal>(OnResourceChanged);
    }


    protected virtual void OnResourceChanged(ResourceChangedSignal signal)
    {
        if (signal.Type == ResourceType)
        {
            UpdateResourceBar(signal.Value);
        }
    }

    protected virtual void UpdateResourceBar(float currentValue)
    {
        _resourceBar.fillAmount = Mathf.Clamp01(currentValue / MaxValue);
    }
}







