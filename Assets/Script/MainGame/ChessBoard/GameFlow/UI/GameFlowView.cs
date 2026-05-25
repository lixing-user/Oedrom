using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class GameFlowView : MonoBehaviour
{
    #region Properties
    [Header("Flow State Display")]
    [SerializeField] private TextMeshProUGUI _flowStateText;
    [SerializeField] private Image _flowStateIcon;
    [SerializeField] private Color _playerTurnColor = Color.blue;
    [SerializeField] private Color _enemyTurnColor = Color.red;


    [Header("Phase State Display")]
    [SerializeField] private TextMeshProUGUI _phaseStateText;
    [SerializeField] private GameObject _phaseIndicatorPanel;


    [Header("Transition Popup")]
    [SerializeField] private GameObject _transitionPopup;
    [SerializeField] private TextMeshProUGUI _transitionText;
    [SerializeField] private CanvasGroup _transitionCanvasGroup;
    [SerializeField] private float _popupDuration = 2f;


    private Sequence _currentAnimation;
    #endregion

    #region Public Methods (由 Presenter 調用)
    /// <summary>
    /// 更新回合狀態顯示
    /// </summary>
    public void UpdateFlowState(Team team, string displayText)
    {
        if (_flowStateText != null)
        {
            _flowStateText.text = displayText;
            _flowStateText.color = team == Team.White ? _playerTurnColor : _enemyTurnColor;

            // 動畫：文字放大效果
            //_flowStateText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
        }

        if (_flowStateIcon != null)
        {
            _flowStateIcon.color = team == Team.White ? _playerTurnColor : _enemyTurnColor;
        }
    }

    /// <summary>
    /// 更新階段狀態顯示
    /// </summary>
    public void UpdatePhaseState(GamePhaseState phase, string displayText)
    {
        if (_phaseStateText != null)
        {
            _phaseStateText.text = displayText;

            // 動畫：文字滑入
            //_phaseStateText.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// 顯示轉換提示彈窗
    /// </summary>
    public void ShowTransitionPopup(string message, FlowTransitionType type)
    {
        if (_transitionPopup == null) return;

        // 停止之前的動畫
        _currentAnimation?.Kill();

        // 設置文字
        if (_transitionText != null)
        {
            _transitionText.text = message;
        }

        // 取得 RectTransform 用於位置動畫
        RectTransform rectTransform = _transitionPopup.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(Screen.width, 0); // 從右側滑入
        Vector2 centerPos = Vector2.zero;

        // 動畫序列
        _currentAnimation = DOTween.Sequence()
            .OnStart(() => {
                _transitionPopup.SetActive(true);
                _transitionCanvasGroup.alpha = 0;
                rectTransform.anchoredPosition = startPos;
            })
            // 快速滑入 + 淡入（0.3秒）
            .Append(rectTransform.DOAnchorPos(centerPos, 0.3f).SetEase(Ease.OutQuad))
            .Join(_transitionCanvasGroup.DOFade(1f, 0.3f))
            // 短暫停留（0.8秒）
            .AppendInterval(0.8f)
            // 快速滑出 + 淡出（0.3秒）
            .Append(rectTransform.DOAnchorPos(new Vector2(-Screen.width, 0), 0.3f).SetEase(Ease.InQuad))
            .Join(_transitionCanvasGroup.DOFade(0f, 0.3f))
            .OnComplete(() => _transitionPopup.SetActive(false));
    }

    /// <summary>
    /// 顯示階段指引文字（例如："請放置卡牌"）
    /// </summary>
    public void ShowPhaseHint(string hint)
    {
        if (_phaseIndicatorPanel != null)
        {
            Debug.Log($"[GameStateView] 階段提示: {hint}");
        }
    }
    #endregion

    #region Private Methods
    private int GetPhaseIndex(GamePhaseState phase)
    {
        return phase switch
        {
            GamePhaseState.StandbyPhase => 0,
            GamePhaseState.MainPhase1 => 1,
            GamePhaseState.MovementPhase => 2,
            GamePhaseState.MainPhase2 => 3,
            GamePhaseState.ResolutionPhase => 4,
            GamePhaseState.EndPhase => 5,
            _ => 0
        };
    }

    private void OnDestroy()
    {
        // 清理動畫
        //_currentAnimation?.Kill();
    }
    #endregion
}
