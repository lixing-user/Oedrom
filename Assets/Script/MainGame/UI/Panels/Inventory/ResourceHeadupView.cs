using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceHeadupView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Resources
    [Header("Container References")]
    [SerializeField] private RectTransform _topRowContainer;    // morale, coin, popularity
    [SerializeField] private RectTransform _bottomRowContainer; // supply
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Top Row (Upper) Positions")]
    [SerializeField] private Vector2 _topStandardPos = Vector2.zero;
    [SerializeField] private Vector2 _topSubtlePos = new Vector2(0, 100); // 往上藏

    [Header("Bottom Row (Bottom) Positions")]
    [SerializeField] private Vector2 _bottomStandardPos = Vector2.zero;
    [SerializeField] private Vector2 _bottomSubtlePos = new Vector2(0, -100); // 往下藏

    [Header("Animation Settings")]
    [SerializeField] private float _standardScale = 1.0f;
    [SerializeField] private float _subtleScale = 0.7f;
    [SerializeField] private float _transitionDuration = 0.3f;
    [SerializeField] private Ease _transitionEase = Ease.OutCubic;
    #endregion

    #region QuestPanel
    [Header("Quest Panel References")]
    [SerializeField] private RectTransform _questPanelRect;
    [SerializeField] private CanvasGroup _questCanvasGroup;

    [Header("Quest Panel Positions")]
    [SerializeField] private Vector2 _questStandardPos = Vector2.zero;
    [SerializeField] private Vector2 _questSubtlePos = new Vector2(200, 0);
    #endregion

    public event Action OnPointerEntered;
    public event Action OnPointerExited;

    private Tweener _currentTween;
    private Sequence _activeSequence;

    private CanvasGroup Group => _canvasGroup ??= GetComponent<CanvasGroup>();

    private void OnDestroy() => _activeSequence?.Kill();

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEntered?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExited?.Invoke();
    }


    public void TransitionTo(UIVisibilityHeadupMode mode, bool immediate = false)
    {
        #region Resource Headup Transition
        _activeSequence?.Kill();
        _activeSequence = DOTween.Sequence();

        bool isStandard = mode == UIVisibilityHeadupMode.Standard;

        Vector2 targetTopPos = isStandard ? _topStandardPos : _topSubtlePos;
        Vector2 targetBottomPos = isStandard ? _bottomStandardPos : _bottomSubtlePos;
        float targetAlpha = isStandard ? 1f : 0.4f;

        if (immediate)
        {
            _topRowContainer.anchoredPosition = targetTopPos;
            _bottomRowContainer.anchoredPosition = targetBottomPos;
            Group.alpha = targetAlpha;
        }
        else
        {
            _activeSequence = DOTween.Sequence();
            _activeSequence.Join(_topRowContainer.DOAnchorPos(targetTopPos, _transitionDuration));
            _activeSequence.Join(_bottomRowContainer.DOAnchorPos(targetBottomPos, _transitionDuration));
            _activeSequence.Join(Group.DOFade(targetAlpha, _transitionDuration));
            _activeSequence.SetEase(_transitionEase);
        }
        #endregion

        #region Quest Panel Transition
        if (_questPanelRect != null && _questCanvasGroup != null)
        {
            Vector2 targetQuestPos = isStandard ? _questStandardPos : _questSubtlePos;
            float targetQuestAlpha = isStandard ? 1f : 0.2f;
            if (immediate)
            {
                _questPanelRect.anchoredPosition = targetQuestPos;
                _questCanvasGroup.alpha = targetQuestAlpha;
            }
            else
            {
                _activeSequence.Join(_questPanelRect.DOAnchorPos(targetQuestPos, _transitionDuration));
                _activeSequence.Join(_questCanvasGroup.DOFade(targetQuestAlpha, _transitionDuration));
            }
            _activeSequence.SetEase(_transitionEase);
        }
        #endregion
    }
}
