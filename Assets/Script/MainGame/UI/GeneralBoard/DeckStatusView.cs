using TMPro;
using UnityEngine;

public class DeckStatusView : MonoBehaviour
{
    [Header("UI °Ñ·Ó")]
    [SerializeField] private TMP_Text _normalCountText;
    [SerializeField] private TMP_Text _brokenCountText;
    [SerializeField] private TMP_Text _capacityText;


    public void UpdateNormalCount(int count)
    {
        if (_normalCountText != null)
            _normalCountText.text = count.ToString();
    }

    public void UpdateBrokenCount(int count)
    {
        if (_brokenCountText != null)
            _brokenCountText.text = count.ToString();
    }

    public void UpdateCapacity(int current, int max)
    {
        if (_capacityText != null)
            _capacityText.text = $"{current} / {max}";
    }
}
