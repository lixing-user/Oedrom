using TMPro;
using UnityEngine;
using Zenject;

public class CoinContainer : MonoBehaviour
{
    private IResourceManager _resourceManager;
    private SignalBus _signalBus;
    [Inject]
    public void Construct(IResourceManager resourceManager, SignalBus signalBus)
    {
        _resourceManager = resourceManager;
        _signalBus = signalBus; 
    }


    [SerializeField] private TMP_Text coinText;


    private void OnEnable()
    {
        float currentCoin = _resourceManager.GetVariable(VariableType.coin);
        UpdateCoinDisplay(currentCoin);

        _signalBus.Subscribe<ResourceChangedSignal>(OnResourceChanged);
    }
    private void OnDisable()
    {
        _signalBus.TryUnsubscribe<ResourceChangedSignal>(OnResourceChanged);
    }


    private void OnResourceChanged(ResourceChangedSignal signal)
    {
        if (signal.Type == VariableType.coin)
        {
            UpdateCoinDisplay(signal.Value);
        }
    }

    public void UpdateCoinDisplay(float currentCoin)
    {
        if (coinText != null)
        {
            coinText.text = $"ª÷¿ú: {Mathf.FloorToInt(currentCoin)}";
        }
    }
}
