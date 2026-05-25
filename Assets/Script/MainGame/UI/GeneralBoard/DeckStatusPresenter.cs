using UnityEngine;
using Zenject;

public class DeckStatusPresenter : MonoBehaviour
{
    [Inject] private readonly IHandService _handService;
    [Inject] private readonly SignalBus _signalBus;

    private DeckStatusView _view;

    void Start()
    {
        _view = GetComponent<DeckStatusView>();

        _signalBus.Subscribe<HandCardsChangedSignal>(RefreshView);
        _signalBus.Subscribe<ResourceChangedSignal>(OnResourceChanged);

        RefreshView();
    }

    void OnDestroy()
    {
        _signalBus.Unsubscribe<HandCardsChangedSignal>(RefreshView);
        _signalBus.Unsubscribe<ResourceChangedSignal>(OnResourceChanged);
    }


    private void RefreshView()
    {
        // 1. 取得資料
        int normalCount = _handService.GetNormalCardCount();
        int brokenCount = _handService.GetBrokenCardCount();
        int currentTotal = _handService.GetCurrentHandCount();
        int maxCapacity = _handService.GetMaxHandSize();

        // 2. 更新 View
        _view.UpdateNormalCount(normalCount);
        _view.UpdateBrokenCount(brokenCount);
        _view.UpdateCapacity(currentTotal, maxCapacity);
    }

    private void OnResourceChanged(ResourceChangedSignal signal)
    {
        if (signal.Type == VariableType.popularity)
        {
            RefreshView();
        }
    }
}
