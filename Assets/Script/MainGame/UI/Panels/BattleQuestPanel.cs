using UnityEngine;
using UnityEngine.UI;


public class BattleQuestPanel : MonoBehaviour, IEventUIPanel
{
    [SerializeField] private Button ConfirmButtom;
    
    public void OnButtonClick()
    {
        Debug.Log("BattleQuestPanel: Confirm button clicked.");
    }
}

internal interface IEventUIPanel
{
    void OnButtonClick();
}