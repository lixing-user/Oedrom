using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhaseControlView : MonoBehaviour, IPhaseControlView
{
    [Header("Phase Control UI")]
    [SerializeField] private RectTransform _buttonControlGroup;
    [SerializeField] private Button _endPhaseButton;

    [Header("Phase Hint UI")]
    [SerializeField] private GameObject _phaseHintPanel;
    [SerializeField] private TextMeshProUGUI _phaseHintText;
    [SerializeField] private CanvasGroup _controlGroup;

    #region Visual Effects
    [Header("Animation Settings")]
    [SerializeField] private Animator _buttonAnimator;
    [SerializeField] private float _rotateDuration = 0.5f;

    [Header("Halo Shader Effect")]
    [SerializeField] private Image _haloImage;
    private Material _haloMat;
    #endregion


    public event System.Action OnEndPhaseRequested;


    void Start()
    {
        _endPhaseButton.onClick.AddListener(() => OnEndPhaseRequested?.Invoke());
        Hide();

        _buttonControlGroup.localRotation = Quaternion.Euler(0, 0, -90f);
        _buttonControlGroup.gameObject.SetActive(false);

        // 複製一份 Shader 材質，避免改到全域材質
        if (_haloImage != null)
        {
            _haloMat = new Material(_haloImage.material);
            _haloImage.material = _haloMat;
            _haloImage.gameObject.SetActive(false); // 預設隱藏光環
        }
    }

    void OnDestroy()
    {
        _endPhaseButton.onClick.RemoveAllListeners();
        _buttonControlGroup.DOKill();
        _haloMat?.DOKill();
        if (_haloMat != null) Destroy(_haloMat);
    }

    #region 業務實作
    public void Show()
    {
        _controlGroup.alpha = 1f;
        _controlGroup.interactable = true;
        _controlGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        _controlGroup.alpha = 0f;
        _controlGroup.interactable = false;
        _controlGroup.blocksRaycasts = false;

        _buttonControlGroup.gameObject.SetActive(false);
    }

    public void SetButtonInteractable(bool interactable)
    {
        _endPhaseButton.interactable = interactable;
    }

    public void ShowPhaseHint(string message)
    {
        _phaseHintText.text = message;
    }
    #endregion

    #region 特效控制
    // 旋轉入場
    public void AnimateShowEndButton()
    {
        _buttonControlGroup.DOKill(); // 停止先前的動畫，防呆
        _buttonControlGroup.gameObject.SetActive(true);

        // 確保從 -90 度開始旋轉到 0 度
        _buttonControlGroup.localRotation = Quaternion.Euler(0, 0, -90f);
        _buttonControlGroup.DOLocalRotate(Vector3.zero, _rotateDuration)
                           .SetEase(Ease.OutBack)
                           .OnComplete(PlayEntranceShrinkEffect);
    }

    // 旋轉出場
    public void AnimateHideEndButton()
    {
        if (!_buttonControlGroup.gameObject.activeSelf) return;

        // 1. 播放向外輻射的光環特效
        PlayClickRadiateEffect();

        _buttonControlGroup.DOKill(); // 防呆

        // 旋轉 Container 到 -90 度後隱藏 Container
        _buttonControlGroup.DOLocalRotate(new Vector3(0, 0, -90f), _rotateDuration)
                           .SetEase(Ease.InBack)
                           .OnComplete(() => _buttonControlGroup.gameObject.SetActive(false));

        StopHighlightEffect();
    }

    // --- 光環 Shader 特效實作 ---
    private void PlayEntranceShrinkEffect()
    {
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
        if (_haloMat == null) return;

        _haloMat.DOKill(); // 強制打斷收縮動畫

        // 從 0.4 瞬間向外輻射到 1.5，擴散完畢後自動隱藏
        _haloMat.DOFloat(1.5f, "_Radius", 0.1f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => _haloImage.gameObject.SetActive(false));
    }

    public void PlayHighlightEffect()
    {
        if (_buttonAnimator != null)
        {
            _buttonAnimator.SetBool("IsHighlighting", true);
        }
    }

    public void StopHighlightEffect()
    {
        if (_buttonAnimator != null)
        {
            _buttonAnimator.SetBool("IsHighlighting", false);
        }
    }
    #endregion
}

public interface IPhaseControlView
{
    event System.Action OnEndPhaseRequested;
    void Show();
    void Hide();

    void AnimateShowEndButton();
    void AnimateHideEndButton();
    void PlayHighlightEffect();
    void StopHighlightEffect();
    void SetButtonInteractable(bool interactable);

    void ShowPhaseHint(string message);
}