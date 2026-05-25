using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Application/DTOs/EventStatusUIData.cs
public struct EventStatusUIData
{
    public Sprite Icon;
    public string Name;
    public string Description;
    public int RemainingTurns;
}


public class StatusItemView : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _turnsText;


    private string _description;

    public void Setup(EventStatusUIData data)
    {
        if (_iconImage != null && data.Icon != null)
            _iconImage.sprite = data.Icon;

        if (_nameText != null)
            _nameText.text = data.Name;

        if (_turnsText != null)
            _turnsText.text = $"{data.RemainingTurns} ¦^¦X";

        _description = data.Description;
    }
}
