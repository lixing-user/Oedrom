using System;
using UnityEngine;
using Zenject;

public class BoardGPresenter : MonoBehaviour
{
    [Inject] private ITutorialOverrideService _overrideService;
    [Inject] private SignalBus _signalBus;
    private IBoardGView _view;

    #region 初始化
    void Start()
    {
        _view = GetComponent<IBoardGView>();

        BindViewEvents();
        _signalBus.Subscribe<ShowGameEndUISignal>(OnBattleEnded);
    }
    void OnDestroy()
    {
        _view.HideEndScreens();
        UnbindViewEvents();
        _signalBus.Unsubscribe<ShowGameEndUISignal>(OnBattleEnded);
    }

    private void BindViewEvents()
    {
        _view.OnCloseResultConfirmed += OnCloseResultButtonClicked;

        _view.OnBackClicked += OnBackClicked;
    }
    private void UnbindViewEvents()
    {
        _view.OnCloseResultConfirmed -= OnCloseResultButtonClicked;

        _view.OnBackClicked += OnBackClicked;
    }
    #endregion

    #region 效果實作
    private void OnBattleEnded(ShowGameEndUISignal signal)
    {
        if (_overrideService.IsFeatureLocked("GameOverUI"))
        {
            Debug.Log("[Presenter] 結算畫面被鎖定，靜默等待教學系統處理...");
            return;
        }

        _view.ShowEndBattleUI();

        Debug.Log($"Winner Team :" + signal.WinnerTeam);
        if (signal.WinnerTeam == Team.White)
            ShowVictoryScreen();
        else
            ShowDefeatScreen();
    }

    private void OnCloseResultButtonClicked()
    {
        // _signalBus發送信號，通知rewardpanel開始運行
        _view.HideEndBattleUI();
    }

    private void OnBackClicked()
    {
        _view?.HideEndScreens();
    }

    private void ShowVictoryScreen()
    {
        Debug.Log("Show Victory Scene");
        _view.ShowVictoryScreen();
    }

    private void ShowDefeatScreen()
    {
        Debug.Log("Show Defeated Scene");
        _view.ShowDefeatScreen();
    }
    #endregion
}
