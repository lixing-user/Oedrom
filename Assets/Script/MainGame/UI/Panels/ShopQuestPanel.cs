using UnityEngine;
using UnityEngine.UI;

public class ShopQuestPanel : MonoBehaviour, IEventUIPanel
{
    [SerializeField] private Button ConfirmButtom;

    public void OnButtonClick()
    {
        Debug.Log("ShopQuestPanel: Confirm button clicked.");
    }
}
