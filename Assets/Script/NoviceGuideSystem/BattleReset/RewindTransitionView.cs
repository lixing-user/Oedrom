using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class RewindTransitionView : MonoBehaviour
{
    [Header("UI 容器")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;

    [Header("Rewind 特效")]
    [SerializeField] private RectTransform _rewindTextTransform;
    [SerializeField] private TMP_Text _rewindText;

    private void Awake()
    {
        HideAllEffects();
    }

    public void PlayRewindEffect(Team winner)
    {
        if (_mainCanvasGroup != null)
        {
            _mainCanvasGroup.alpha = 1f;
            _mainCanvasGroup.blocksRaycasts = true;
            // 背景黑幕平滑淡入 (0.3秒)
            _mainCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        }

        if (_rewindTextTransform != null && _rewindText != null)
        {
            _rewindTextTransform.gameObject.SetActive(true);

            // 為了通用性，不論誰贏，回溯動畫都採用統一的視覺顏色 (例如藍色/黃色)
            _rewindText.color = Color.cyan;
            _rewindText.text = "REWIND";

            // 文字瞬間放大後縮回原比例，增加打擊感
            _rewindTextTransform.localScale = Vector3.one * 5f;
            _rewindTextTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBounce).SetUpdate(true);
        }
    }

    public void FadeOutEffects(Action onComplete = null)
    {
        if (_mainCanvasGroup == null && _rewindTextTransform == null)
        {
            // 防呆：如果沒有任何 UI 元素，也要確保 Callback 被執行，以免流程卡死
            onComplete?.Invoke();
            return;
        }
        // 停止進場時的動畫，防止互相干擾
        _rewindTextTransform.DOKill();

        if (_mainCanvasGroup != null)
        {
            // 黑幕平滑淡出 (0.5秒)，完全透明後再把整個物件關閉
            _mainCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                HideAllEffects();
                onComplete?.Invoke();
            });
        }
        else
        {
            // 防呆：如果沒有 CanvasGroup，也要確保 Callback 被執行，以免流程卡死
            HideAllEffects();
            onComplete?.Invoke();
        }
    }

    public void HideAllEffects()
    {
        if (_mainCanvasGroup != null)
        {
            _mainCanvasGroup.alpha = 0f;
            _mainCanvasGroup.blocksRaycasts = false;
            _mainCanvasGroup.interactable = false;
        }

        // 分別進行防呆與隱藏/清除動畫
        if (_rewindTextTransform != null)
        {
            _rewindTextTransform.gameObject.SetActive(false);
            _rewindTextTransform.DOKill();
        }
    }
}
