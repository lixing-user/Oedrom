using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

// View: 純UI視圖，只負責UI渲染和用戶輸入事件
public class OptionPanel : MonoBehaviour
{
    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    public event Action<float> OnVolumeChangeRequested;
    public event Action<int> OnQualityChangeRequested;
    public event Action<int> OnResolutionChangeRequested;
    public event Action<bool> OnFullscreenChangeRequested;
    public event Action OnCloseRequested;

    private void Start()
    {
        BindUIEvents();
    }
    private void OnDestroy()
    {
        UnbindUIEvents();
    }


    #region Private Methods - 綁定與解除綁定UI事件
    private void BindUIEvents()
    {
        volumeSlider.onValueChanged.AddListener(value => OnVolumeChangeRequested?.Invoke(value));
        qualityDropdown.onValueChanged.AddListener(value => OnQualityChangeRequested?.Invoke(value));
        resolutionDropdown.onValueChanged.AddListener(value => OnResolutionChangeRequested?.Invoke(value));
        fullscreenToggle.onValueChanged.AddListener(value => OnFullscreenChangeRequested?.Invoke(value));

        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    private void UnbindUIEvents()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveAllListeners();

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveAllListeners();

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveAllListeners();

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Public Methods - 更新UI顯示
    public void UpdateVolumeSlider(float value)
    {
        volumeSlider.SetValueWithoutNotify(value);
    }
    public void UpdateQualityDropdown(int index)
    {
        qualityDropdown.SetValueWithoutNotify(index);
    }
    public void UpdateResolutionDropdown(int index)
    {
        resolutionDropdown.SetValueWithoutNotify(index);
    }
    public void UpdateFullscreenToggle(bool isFullscreen)
    {
        fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
    }
    private void OnCloseButtonClicked()
    {
        OnCloseRequested?.Invoke();
        Debug.Log("Close button clicked in OptionPanel.");
    }
    #endregion

    public void InitializeResolutionOptions(Resolution[] resolutions, int currentIndex)
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        foreach (var resolution in resolutions)
        {
            options.Add($"{resolution.width} x {resolution.height}");
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }



    public void Show()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
