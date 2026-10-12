using DG.Tweening;
using System.Collections.Generic;
using TMPro;
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


    [Header("Transition Popups")]
    [SerializeField] private ScaleFadePopup _turnPopup;
    [SerializeField] private SlidePopup _phasePopup;


    private readonly Queue<string> _turnQueue = new Queue<string>();
    private string _pendingPhaseMessage;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (_turnPopup == null || _phasePopup == null)
        {
            Debug.LogError("[GameFlowView] 請在 Inspector 指定 _turnPopup 與 _phasePopup。", this);
            return;
        }

        _turnPopup.Stop();
        _phasePopup.Stop();
    }
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
    /// 顯示回合提示（遊戲開始 / 你的回合 / 敵方回合）
    /// </summary>
    public void ShowTurnPopup(string message)
    {
        if (_turnPopup == null || _phasePopup == null) return;

        // 新回合開始：舊的階段提示（播放中或等待中）都已過期，一律作廢
        _phasePopup.Stop();
        _pendingPhaseMessage = null;

        _turnQueue.Enqueue(message);
        if (!_turnPopup.IsPlaying) PlayNextTurn();
    }

    /// <summary>
    /// 顯示階段提示。回合提示播放中會延後顯示，且只保留最新一則。
    /// </summary>
    public void ShowPhasePopup(string message)
    {
        if (_turnPopup == null || _phasePopup == null) return;

        if (_turnPopup.IsPlaying)
        {
            _pendingPhaseMessage = message;
            return;
        }

        _phasePopup.Play(message);   // 播放中再呼叫會直接取代，符合「只留最新」
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
    private void PlayNextTurn()
    {
        if (_turnQueue.Count == 0)
        {
            FlushPendingPhase();   // 回合提示全部播完，才輪到等待中的階段提示
            return;
        }

        _turnPopup.Play(_turnQueue.Dequeue(), PlayNextTurn);
    }

    private void FlushPendingPhase()
    {
        if (_pendingPhaseMessage == null) return;

        string message = _pendingPhaseMessage;
        _pendingPhaseMessage = null;
        _phasePopup.Play(message);
    }
    #endregion
}
