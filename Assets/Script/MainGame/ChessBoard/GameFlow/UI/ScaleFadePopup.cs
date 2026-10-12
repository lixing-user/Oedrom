using DG.Tweening;
using UnityEngine;

/// <summary>
/// Turn Control 的縮放淡入淡出提示框
/// </summary>
public class ScaleFadePopup : TransitionPopup
{
    [SerializeField] private Vector2 _centerPosition = Vector2.zero;
    [SerializeField] private float _startScale = 0.5f;   // 入場從小放大
    [SerializeField] private float _exitScale = 1.15f;   // 退場稍微放大並淡出
    [SerializeField] private float _enterTime = 0.25f;
    [SerializeField] private float _exitTime = 0.25f;

    protected override void ResetToStart()
    {
        _canvasGroup.alpha = 0f;
        Rect.anchoredPosition = _centerPosition;
        Rect.localScale = Vector3.one * _startScale;
    }

    protected override void AppendEnter(Sequence seq)
    {
        seq.Append(Rect.DOScale(1f, _enterTime).SetEase(Ease.OutBack))   // 輕微過衝，有「彈出」的感覺
           .Join(_canvasGroup.DOFade(1f, _enterTime));
    }

    protected override void AppendExit(Sequence seq)
    {
        seq.Append(Rect.DOScale(_exitScale, _exitTime).SetEase(Ease.InQuad))
           .Join(_canvasGroup.DOFade(0f, _exitTime));
    }
}
