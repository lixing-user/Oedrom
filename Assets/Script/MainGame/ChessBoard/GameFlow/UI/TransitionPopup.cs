using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public abstract class TransitionPopup : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _text;
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected float _holdTime = 0.8f;

    private Sequence _sequence;
    public bool IsPlaying { get; private set; }

    protected RectTransform Rect => (RectTransform)transform;

    // 子類別實作：設定入場前的初始狀態
    protected abstract void ResetToStart();
    // 子類別實作：把入場 / 退場 tween 加進 sequence
    protected abstract void AppendEnter(Sequence seq);
    protected abstract void AppendExit(Sequence seq);

    public void Play(string message, Action onFinished = null)
    {
        if (_text == null || _canvasGroup == null)
        {
            Debug.LogError($"[{name}] 缺少 _text 或 _canvasGroup 參考。", this);
            onFinished?.Invoke();   // 讓外部佇列繼續往下走，不會卡死
            return;
        }

        Stop();

        _text.text = message;
        gameObject.SetActive(true);
        ResetToStart();
        IsPlaying = true;

        var seq = DOTween.Sequence();
        AppendEnter(seq);
        seq.AppendInterval(_holdTime);
        AppendExit(seq);

        seq.SetLink(gameObject)
           .OnComplete(() =>
           {
               IsPlaying = false;
               gameObject.SetActive(false);
               onFinished?.Invoke();   // Kill() 不會觸發這裡，Stop() 不會誤觸下一個
           });

        _sequence = seq;
    }

    public void Stop()
    {
        _sequence?.Kill();
        _sequence = null;
        IsPlaying = false;
        gameObject.SetActive(false);
    }
}