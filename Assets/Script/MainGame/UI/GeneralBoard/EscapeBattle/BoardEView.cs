using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class BoardEView : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PanelView setupPanel;
    [SerializeField] public ButtonView startGameButton;

    [Header("UI 元件綁定")]
    [SerializeField] private GameObject _TimerBarPrefab; 
    [SerializeField] private Image _TimerBarPrefabImage;

    [Header("Resolution")]
    [SerializeField] private PanelView EndGamePanel;
    [SerializeField] private ButtonView EndGameButton;

    [Header("Halo Shader Effect")]
    [SerializeField] private Image _haloImage;
    private Material _haloMat;

    [Header("Lighting")]
    [SerializeField] private Light _battlefieldSpotLight;
    [SerializeField] private Light _sideAssistLight01;
    [SerializeField] private Light _sideAssistLight02;
    [SerializeField] private float _setupDimIntensity = 0.0f; // 佈陣時的暗度
    [SerializeField] private float _normalIntensity = 100.0f;   // 遊戲開始時的正常亮度
    [SerializeField] private float _lightFadeDuration = 0.4f; // 燈光漸變時間


    public event System.Action OnStartGameClicked;
    public event System.Action OnEndGameRequest;

    private void Start()
    {
        startGameButton.OnClicked += () => OnStartGameClicked?.Invoke();
        EndGameButton.OnClicked += () => OnEndGameRequest?.Invoke();

        // 2. 複製一份 Shader 材質，避免改到全域材質，並確保預設隱藏
        EnsureHaloMaterial();
    }
    private void OnDestroy()
    {
        startGameButton.OnClicked -= () => OnStartGameClicked?.Invoke();
        EndGameButton.OnClicked -= () => OnEndGameRequest?.Invoke();
    }

    #region Setup Panel
    public void ShowSetupPanel()
    {
        setupPanel.Show();
        PlayEntranceShrinkEffect();
        SetSetupLighting();
    }
    public void HideSetupPanel()
    {
        PlayClickRadiateEffect();
        setupPanel.Hide();
        SetBattlefieldLighting();
    }

    // --- 光環 Shader 特效實作 ---
    private void EnsureHaloMaterial()
    {
        if (_haloMat != null || _haloImage == null) return;

        _haloMat = new Material(_haloImage.material);
        _haloImage.material = _haloMat;
        _haloImage.gameObject.SetActive(false); // 補回預設隱藏,行為與 Phase 一致
    }


    private void PlayEntranceShrinkEffect()
    {
        EnsureHaloMaterial();
        if (_haloMat == null) return;

        _haloImage.gameObject.SetActive(true);
        _haloMat.DOKill();

        // 假設 Shader 中的半徑變數為 _Radius，從 1.0 縮小到 0.4
        _haloMat.SetFloat("_Radius", 0.8f);
        _haloMat.DOFloat(0.2f, "_Radius", 1.5f)
                .SetEase(Ease.OutQuad)
                .SetLoops(-1, LoopType.Restart);
    }

    private void PlayClickRadiateEffect()
    {
        EnsureHaloMaterial();
        if (_haloMat == null) return;

        _haloMat.DOKill(); // 強制打斷收縮動畫

        // 從 0.4 瞬間向外輻射到 1.5，擴散完畢後自動隱藏
        _haloMat.DOFloat(1.5f, "_Radius", 0.1f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => _haloImage.gameObject.SetActive(false));
    }
    #endregion

    #region Timer Countdown
    public void InitializeTimerBar()
    {
        if (_TimerBarPrefabImage != null)
        {
            _TimerBarPrefabImage.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("[BoardEView] 在生成的計時條上找不到 Image 元件！請檢查 Prefab。");
        }
    }


    public void UpdateTimerDisplay(float remainingTime, float duration)
    {
        if (_TimerBarPrefabImage == null || duration <= 0f) return;

        // 計算剩餘時間的比例 (會得到 0.0 ~ 1.0 的浮點數)
        float fillRatio = remainingTime / duration;

        // 更新 UI 的 Fill Amount 數值
        _TimerBarPrefabImage.fillAmount = fillRatio;

        // (額外視覺優化) 當時間剩下不到 20% 時，可以讓計時條變成紅色警告玩家
        if (fillRatio <= 0.2f)
        {
            _TimerBarPrefabImage.color = Color.red;
        }
        else
        {
            _TimerBarPrefabImage.color = Color.white; // 預設顏色
        }
    }

    public void PlayTimeUpEffect()
    {
        Debug.Log("[BoardEView] 時間到！播放結束特效！");
        // 這裡可以加入例如畫面震動、計時條閃爍等 Animator 觸發
    }
    #endregion

    #region Resolution
    public void ShowEndGamePanel()
    {
        EndGamePanel.Show();
    }
    public void HideEndGamePanel()
    {
        EndGamePanel.Hide();
    }
    #endregion

    #region Lighting Control
    private void SetSetupLighting()
    {
        if (_battlefieldSpotLight != null)
        {
            _battlefieldSpotLight.DOKill();
            _battlefieldSpotLight.DOIntensity(_setupDimIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
            _sideAssistLight01.DOIntensity(_setupDimIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
            _sideAssistLight02.DOIntensity(_setupDimIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
        }
    }

    private void SetBattlefieldLighting()
    {
        if (_battlefieldSpotLight != null)
        {
            _battlefieldSpotLight.DOKill();
            _battlefieldSpotLight.DOIntensity(_normalIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
            _sideAssistLight01.DOIntensity(_normalIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
            _sideAssistLight02.DOIntensity(_normalIntensity, _lightFadeDuration).SetEase(Ease.OutCubic);
        }
    }
    #endregion
}
