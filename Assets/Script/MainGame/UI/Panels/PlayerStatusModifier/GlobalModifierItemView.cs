using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct ModifierUIData
{
    public Sprite Icon;
    public string Name;
    public int RemainingBattles;
    public Color DisplayColor;
}

public class GlobalModifierItemView : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _countText;

    public void Setup(ModifierUIData data)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = data.Icon;
            _iconImage.color = data.DisplayColor;
        }

        if (_nameText != null)
            _nameText.text = data.Name; 

        if (_countText != null)
            _countText.text = $"{data.RemainingBattles} ¦¸";
    }
}
