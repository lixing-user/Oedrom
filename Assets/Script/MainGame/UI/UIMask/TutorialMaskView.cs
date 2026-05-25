using Coffee.UIExtensions;
using UnityEngine;

public class TutorialMaskView : MonoBehaviour
{
    [SerializeField] private GameObject _maskPanel;
    [SerializeField] private CanvasGroup _maskCanvasGroup;
    [SerializeField] private Unmask _unmaskComponent;


    // 給 3D 棋盤用的：手動指定螢幕座標與大小
    public void FocusOnScreenPosition(Vector2 screenPosition, Vector2 sizeDelta)
    {
        //_maskPanel.SetActive(true);
        Show();

        var rect = _unmaskComponent.transform as RectTransform;
        rect.position = screenPosition;
        rect.sizeDelta = sizeDelta;
    }

    // 給 UI 卡牌用的：利用源碼內建的 FitTarget 功能自動追蹤
    public void FocusOnUIElement(RectTransform targetUI)
    {
        Show();

        // 自動對齊並跟隨目標 UI
        _unmaskComponent.fitTarget = targetUI;
    }

    public void FocusOnAnchoredPosition(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        //_maskPanel.SetActive(true);
        Show();
        _unmaskComponent.fitTarget = null;

        var rect = _unmaskComponent.transform as RectTransform;

        // 設定錨點 (例如 Bottom Center 就是 0.5, 0)
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 設定相對於錨點的偏移量與大小
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
    }

    public void Show()
    {
        _maskCanvasGroup.alpha = 1f;
        _maskCanvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        _maskCanvasGroup.alpha = 0f;
        _maskCanvasGroup.blocksRaycasts = false;
    }
}
