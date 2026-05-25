using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Button button;

    [Header("Option Content (Optional)")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;

    [Header("animation settings")]
    [SerializeField] private float pressscale = 0.9f;
    [SerializeField] private float animationduration = 0.1f;

    public event System.Action OnClicked;

    private void Start()
    {
        button ??= GetComponent<Button>();
        button.onClick.AddListener(HandleClick);

        //其他元素初始化（視覺、音效）
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);

        //其他元素清理（視覺、音效）
        //transform.DOKill(); // 清理 DOTween 動畫
    }


    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {
    }

    private void HandleClick()
    {
        Debug.Log($"Button {gameObject.name} clicked.");
        OnClicked?.Invoke();
    }

    // --- 供外部清空事件，防止重複使用時發生事件疊加 ---
    public void ClearListeners()
    {
        OnClicked = null;
    }

    // --- 供 BoardTView 設定選項文字 ---
    public void SetContent(string title, string description, Sprite icon = null)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
    }
}
