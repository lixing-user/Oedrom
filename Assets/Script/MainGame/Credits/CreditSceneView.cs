using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditSceneView : MonoBehaviour
{
    [Header("ScrollPanel")]
    [SerializeField] private RectTransform _scrollContainer;
    [SerializeField] private TextMeshProUGUI _scrollText;

    [Header("SlidePanel")]
    [SerializeField] private CanvasGroup[] _slidePanels;

    [Header("Playback Settings")]
    [Tooltip("捲動名單的移動速度 (數值越大跑越快)")]
    [SerializeField] private float _scrollSpeed = 50f;
    [SerializeField] private int _windowHeight = 1080;

    [Tooltip("每張投影片在畫面上的停留秒數 (不含淡入淡出時間)")]
    [SerializeField] private float _slideDuration = 3f;

    private Action _onSequenceComplete;


    public void ResetView()
    {
        foreach (var panel in _slidePanels)
        {
            panel.alpha = 0;
            panel.gameObject.SetActive(false); // 節省效能
        }
        _scrollContainer.anchoredPosition = new Vector2(0, -1000);
    }


    // 播放單頁 Slide
    public Sequence PlaySlide(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= _slidePanels.Length)
            return DOTween.Sequence(); // 防呆


        CanvasGroup target = _slidePanels[panelIndex];
        target.gameObject.SetActive(true);


        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOFade(1, 1f));
        seq.AppendInterval(_slideDuration);
        seq.Append(target.DOFade(0, 1f));
        seq.OnComplete(() => target.gameObject.SetActive(false)); // 播完隱藏
        return seq;
    }


    // 播放捲動名單
    public Sequence PlayScroll(string textContent)
    {
        _scrollText.text = textContent;

        // 根據文字長度計算需要捲動的距離
        float targetY = _scrollText.preferredHeight + _windowHeight; // 假設螢幕高 1080
        float duration = targetY / _scrollSpeed;

        Sequence seq = DOTween.Sequence();
        seq.Append(_scrollContainer.DOAnchorPosY(targetY, duration).SetEase(Ease.Linear));
        return seq;
    }

    public void CompleteSequence()
    {
        _onSequenceComplete?.Invoke();
    }
}
