using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

// Presenter: 協調Model和View之間的互動，處理業務邏輯
public class OptionPanelHandler : MonoBehaviour
{
    [Inject] private GameSettingManager _settingManager;

    private OptionPanel _optionPanel;

    public static OptionPanelHandler Instance { get; private set; }

    #region 初始化

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromView();
        UnsubscribeFromModel();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DeferredBind());
    }
    private IEnumerator DeferredBind()
    {
        yield return null; // 等待一幀，讓場景完全初始化
        FindAndBindOptionPanel();
    }

    public void FindAndBindOptionPanel()
    {
        // 嘗試查找 SettingPanelUI
        _optionPanel = FindObjectOfType<OptionPanel>();

        if (_optionPanel != null)
        {
            BindView();
            BindModel();
            InitializeView();
            CloseOptionPanel();
            Debug.Log("OptionPanel found and assigned.");
        }
        else
        {
            Debug.LogWarning("SettingPanelUI not found in the current scene.");
        }
    }
    private void InitializeView()
    {
        if (_optionPanel == null || _settingManager == null) return;

        _optionPanel.UpdateVolumeSlider(_settingManager.Volume);
        _optionPanel.UpdateQualityDropdown(_settingManager.QualityIndex);
        _optionPanel.UpdateFullscreenToggle(_settingManager.IsFullscreen);

        // 初始化解析度選項
        int currentResolutionIndex = _settingManager.GetCurrentResolutionIndex();
        _optionPanel.InitializeResolutionOptions(_settingManager.Resolutions, currentResolutionIndex);
    }
    #endregion

    #region Private Methods _ 處理ModelView事件
    private void OnVolumeChanged(float value)
    {
        _settingManager?.SetVolume(value);
    }
    private void OnQualityChanged(int value)
    {
        _settingManager?.SetQuality(value);
    }
    private void OnResolutionChanged(int value)
    {
        _settingManager?.SetResolution(value);
    }
    private void OnFullscreenChanged(bool value)
    {
        _settingManager?.SetFullscreen(value);
    }

    private void UpdateViewVolume(float value)
    {
        _optionPanel?.UpdateVolumeSlider(value);
    }
    private void UpdateViewQuality(int index)
    {
        _optionPanel?.UpdateQualityDropdown(index);
    }
    private void UpdateViewResolution(int index)
    {
        _optionPanel?.UpdateResolutionDropdown(index);
    }
    private void UpdateViewFullscreen(bool isFullscreen)
    {
        _optionPanel?.UpdateFullscreenToggle(isFullscreen);
    }
    #endregion

    #region Private Methods _ Bind View & Model
    private void BindView()
    {
        UnsubscribeFromView(); // 先取消訂閱避免重複

        _optionPanel.OnVolumeChangeRequested += OnVolumeChanged;
        _optionPanel.OnQualityChangeRequested += OnQualityChanged;
        _optionPanel.OnResolutionChangeRequested += OnResolutionChanged;
        _optionPanel.OnFullscreenChangeRequested += OnFullscreenChanged;
        _optionPanel.OnCloseRequested += CloseOptionPanel;
    }
    private void UnsubscribeFromView()
    {
        if (_optionPanel != null)
        {
            _optionPanel.OnVolumeChangeRequested -= OnVolumeChanged;
            _optionPanel.OnQualityChangeRequested -= OnQualityChanged;
            _optionPanel.OnResolutionChangeRequested -= OnResolutionChanged;
            _optionPanel.OnFullscreenChangeRequested -= OnFullscreenChanged;
            _optionPanel.OnCloseRequested -= CloseOptionPanel;
        }
    }
    private void BindModel()
    {
        if (_settingManager != null)
        {
            _settingManager.OnVolumeChanged += UpdateViewVolume;
            _settingManager.OnQualityChanged += UpdateViewQuality;
            _settingManager.OnResolutionChanged += UpdateViewResolution;
            _settingManager.OnFullscreenChanged += UpdateViewFullscreen;
        }
    }
    private void UnsubscribeFromModel()
    {
        if (_settingManager != null)
        {
            _settingManager.OnVolumeChanged -= UpdateViewVolume;
            _settingManager.OnQualityChanged -= UpdateViewQuality;
            _settingManager.OnResolutionChanged -= UpdateViewResolution;
            _settingManager.OnFullscreenChanged -= UpdateViewFullscreen;
        }
    }
    #endregion

    #region Public Methods _ View管理
    public void OpenOptionPanel()
    {
        _optionPanel?.Show();
    }
    public void CloseOptionPanel()
    {
        _optionPanel?.Hide();
    }
    #endregion
}
