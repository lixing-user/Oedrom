using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;


public enum HandDisplayMode { Centered, BottomRight }

public class HandUIInputDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Inject] private IHandUIStateManager _stateManager;

    [Header("Input Settings")]
    [Tooltip("雙擊時間閾值（秒）")]
    [SerializeField] private float doubleClickTime = 0.3f; 

    [Tooltip("滾輪靈敏度閾值，避免輕微觸碰就觸發")]
    [SerializeField] private float scrollThreshold = 0.05f;

    [Tooltip("是否只在滑鼠懸停於此 UI 區域時才允許操作？")]
    [SerializeField] private bool requireHover = false;

    private float _lastClickTime = 0f;
    private bool _isHovering = false;


    private void Update()
    {
        // 如果設定為需要懸停，且滑鼠不在區域內，則不處理輸入
        if (requireHover && !_isHovering) return;

        HandleDoubleClick();
        HandleScrollWheel();
    }

    private void HandleDoubleClick()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = 滑鼠左鍵
        {
            // 使用 unscaledTime 確保遊戲暫停時 UI 仍能正常計算時間
            float timeSinceLastClick = Time.unscaledTime - _lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                // 觸發雙擊
                _stateManager.ToggleHandMode();

                // 重置點擊時間，避免連續點三下觸發兩次雙擊
                _lastClickTime = 0f;
            }
            else
            {
                // 記錄第一次點擊的時間
                _lastClickTime = Time.unscaledTime;
            }
        }
    }


    private void SetHandModeExplicitly(HandDisplayMode newMode)
    {
        // 注意：這裡假設您的 IHandUIStateManager 有提供明確設定 Mode 的方法
        // 如果沒有，您需要在 StateManager 中新增這個方法，或者透過 Zenject 的 SignalBus 發送 Signal
        _stateManager.SetHandDisplayMode(newMode);
    }


    private void HandleScrollWheel()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        // 避免微小震動觸發
        if (Mathf.Abs(scrollDelta) > scrollThreshold)
        {
            if (scrollDelta > 0)
            {
                // 向上滾動：設定為 Centered
                SetHandModeExplicitly(HandDisplayMode.Centered);
            }
            else
            {
                // 向下滾動：設定為 BottomRight
                SetHandModeExplicitly(HandDisplayMode.BottomRight);
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
    }
}

public class SceneUIReadySignal { }

public struct ToggleHandDeckSignal { }

public struct ChangeHandUIVisibilitySignal
{
    public UIVisibilityHeadupMode VisibilityMode;
    public HandDisplayMode DisplayMode;
}