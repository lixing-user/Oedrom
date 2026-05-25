using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 故事模式專用按鍵
/// </summary>
public class ButtonViewS : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Button button;

    [Header("Animation Settings")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float animationDuration = 0.1f;

    public Color colors;
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
}
