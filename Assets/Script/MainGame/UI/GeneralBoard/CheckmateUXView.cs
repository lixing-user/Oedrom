using DG.Tweening;
using TMPro;
using UnityEngine;

public class CheckmateUXView : MonoBehaviour
{
    [Header("UI 容器")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;

    [Header("Checkmate 特效")]
    [SerializeField] private RectTransform _checkmateTextTransform;
    [SerializeField] private TMP_Text _checkmateText;

    [Header("Escape 特效")]
    [SerializeField] private RectTransform _escapeTextTransform;
    [SerializeField] private TMP_Text _escapeText;


    private void Awake()
    {
        HideAllEffects();
    }

    public void PlayCheckmateEffect(Team winner)
    {
        if (_mainCanvasGroup != null) _mainCanvasGroup.alpha = 1f;

        if (_checkmateTextTransform != null && _checkmateText != null)
        {
            _checkmateTextTransform.gameObject.SetActive(true);

            _checkmateText.color = (winner == Team.White) ? Color.yellow : Color.red;
            _checkmateText.text = (winner == Team.White) ? "CHECKMATE" : "DEFEATED";

            _checkmateTextTransform.localScale = Vector3.one * 5f;
            _checkmateTextTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBounce).SetUpdate(true);
        }

        //if (_checkmateParticles != null)
        //{
        //    _checkmateParticles.Play();
        //}
    }

    public void PlayEscapeEffect()
    {
        if (_mainCanvasGroup != null) _mainCanvasGroup.alpha = 1f;

        if (_escapeTextTransform != null)
        {
            _escapeTextTransform.gameObject.SetActive(true);

            _escapeTextTransform.anchoredPosition = new Vector2(0, -200f);
            _escapeTextTransform.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);

            _escapeTextTransform.localScale = Vector3.one * 0.5f;
            _escapeTextTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        //if (_escapeParticles != null)
        //{
        //    _escapeParticles.Play();
        //}
    }

    public void HideAllEffects()
    {
        if (_mainCanvasGroup != null) _mainCanvasGroup.alpha = 0f;

        // 分別進行防呆與隱藏/清除動畫
        if (_checkmateTextTransform != null)
        {
            _checkmateTextTransform.gameObject.SetActive(false);
            _checkmateTextTransform.DOKill();
        }

        if (_escapeTextTransform != null)
        {
            _escapeTextTransform.gameObject.SetActive(false);
            _escapeTextTransform.DOKill();
        }
    }
}
