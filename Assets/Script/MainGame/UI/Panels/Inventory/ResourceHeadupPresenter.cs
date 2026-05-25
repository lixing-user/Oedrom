using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public enum UIVisibilityHeadupMode
{
    Standard,
    Subtle
}

public class ResourceHeadupPresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;

    private ResourceHeadupView _view;
    private UIVisibilityHeadupMode? _currentMode = null;

    private void Awake()
    {
        _view = GetComponent<ResourceHeadupView>();
        if (_view == null)
        {
            Debug.LogError("ResourceHeadupView reference is missing in ResourceHeadupPresenter.");
        }
    }

    private void Start()
    {
        _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
        SetMode(UIVisibilityHeadupMode.Subtle, true);
    }
    private void OnDestroy()
    {
        _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
        _view = null;
    }

    private void OnEnable()
    {
        _view.OnPointerEntered += HandlePointerEntered;
        _view.OnPointerExited += HandlePointerExited;
    }

    private void OnDisable()
    {
        _view.OnPointerEntered -= HandlePointerEntered;
        _view.OnPointerExited -= HandlePointerExited;
    }

    private void OnGameStarted()
    {
        SetMode(UIVisibilityHeadupMode.Standard);
    }

    private void HandlePointerEntered()
    {
        SetMode(UIVisibilityHeadupMode.Standard);
    }

    private void HandlePointerExited()
    {
        SetMode(UIVisibilityHeadupMode.Subtle);
    }


    private void SetMode(UIVisibilityHeadupMode mode, bool immediate = false)
    {
        if (_currentMode == mode && !immediate) return;

        _currentMode = mode;
        _view.TransitionTo(mode, immediate);
    }
}
