using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;

    [Header("Image Animation Settings")]
    [SerializeField] private RectTransform leftImage;
    [SerializeField] private RectTransform rightImage;
    [SerializeField] private Vector2 moveOffset = new Vector2(500f, 500f);
    [SerializeField] private float moveTime = 0.5f;

    private Vector2 _leftOriginalPos;
    private Vector2 _rightOriginalPos;


    private void Awake()
    {
        if (leftImage != null) _leftOriginalPos = leftImage.anchoredPosition;
        if (rightImage != null) _rightOriginalPos = rightImage.anchoredPosition;
    }

    public IEnumerator FadeIn()
    {
        // 1. 圖片先設定在起始位置 (左圖在左下，右圖在右上)
        if (leftImage != null) leftImage.anchoredPosition = _leftOriginalPos - moveOffset;
        if (rightImage != null) rightImage.anchoredPosition = _rightOriginalPos + moveOffset;

        // 2. 先執行 FadeTo (暗幕漸顯)，並等待完成
        yield return FadeTo(1f, fadeInTime);

        // 3. 後執行圖片入場，並等待完成
        yield return MoveImages(true, moveTime);
    }

    public IEnumerator FadeOut()
    {
        // 1. 先執行 FadeTo (暗幕漸隱)，並等待完成
        yield return FadeTo(0f, fadeOutTime);

        // 2. 後執行圖片出場 (繼續向前往右上/左下移動)，並等待完成
        yield return MoveImages(false, moveTime);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator MoveImages(bool isEnter, float duration)
    {
        // 使用 DOTween 的 Sequence 將左右圖片的動畫綁在一起同時執行
        Sequence moveSeq = DOTween.Sequence();

        if (isEnter)
        {
            // 入場：移動至定格位置
            if (leftImage != null)
                moveSeq.Join(leftImage.DOAnchorPos(_leftOriginalPos, duration).SetEase(Ease.OutCubic));
            if (rightImage != null)
                moveSeq.Join(rightImage.DOAnchorPos(_rightOriginalPos, duration).SetEase(Ease.OutCubic));
        }
        else
        {
            // 出場：繼續往前移動 (左圖往右上，右圖往左下)
            if (leftImage != null)
                moveSeq.Join(leftImage.DOAnchorPos(_leftOriginalPos + moveOffset, duration).SetEase(Ease.InCubic));
            if (rightImage != null)
                moveSeq.Join(rightImage.DOAnchorPos(_rightOriginalPos - moveOffset, duration).SetEase(Ease.InCubic));
        }

        // 等待這組移動動畫完成
        yield return moveSeq.WaitForCompletion();
    }
}
