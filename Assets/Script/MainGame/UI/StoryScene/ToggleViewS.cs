using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleViewS : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image targetImage;   // 要換 sprite 的圖
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;     // 自動播放中的專用 sprite

    public event Action<bool> OnValueChanged;

    private void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(HandleValueChanged);
        ApplyVisual(toggle.isOn);
    }

    private void OnDestroy() => toggle.onValueChanged.RemoveListener(HandleValueChanged);

    private void HandleValueChanged(bool isOn)
    {
        ApplyVisual(isOn);
        OnValueChanged?.Invoke(isOn);

        // 避免之後按 Space / Enter 再次觸發 Submit
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // 給 Presenter 同步狀態用，不會觸發事件
    public void SetIsOnWithoutNotify(bool isOn)
    {
        toggle.SetIsOnWithoutNotify(isOn);
        ApplyVisual(isOn);
    }

    private void ApplyVisual(bool isOn)
    {
        if (targetImage != null) targetImage.sprite = isOn ? onSprite : offSprite;
    }
}
