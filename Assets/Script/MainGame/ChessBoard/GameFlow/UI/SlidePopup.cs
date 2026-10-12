using DG.Tweening;
using UnityEngine;

/// <summary>
/// Phase Control 的滑入滑出提示框
/// </summary>
public class SlidePopup : TransitionPopup
{
    [SerializeField] private Vector2 _restPosition = Vector2.zero;
    [SerializeField] private float _slideTime = 0.3f;

    private float ParentWidth => ((RectTransform)Rect.parent).rect.width;

    protected override void ResetToStart()
    {
        _canvasGroup.alpha = 0f;
        Rect.anchoredPosition = new Vector2(ParentWidth, _restPosition.y);
    }

    protected override void AppendEnter(Sequence seq)
    {
        seq.Append(Rect.DOAnchorPos(_restPosition, _slideTime).SetEase(Ease.OutQuad))
           .Join(_canvasGroup.DOFade(1f, _slideTime));
    }

    protected override void AppendExit(Sequence seq)
    {
        seq.Append(Rect.DOAnchorPos(new Vector2(-ParentWidth, _restPosition.y), _slideTime).SetEase(Ease.InQuad))
           .Join(_canvasGroup.DOFade(0f, _slideTime));
    }
}
