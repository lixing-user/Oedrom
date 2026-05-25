using UnityEngine;

public class ShopItem3DClickProxy : MonoBehaviour
{
    public string ItemId;
    public event System.Action<string> OnClicked;

    // 依賴於掛載物件上的 Collider
    private void OnMouseUpAsButton()
    {
        OnClicked?.Invoke(ItemId);
    }
}
