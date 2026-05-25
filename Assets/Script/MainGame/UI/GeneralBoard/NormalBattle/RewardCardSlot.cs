using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject brokenOverlay;
    [SerializeField] private GameObject selectionOverlay;
    [SerializeField] private Toggle toggle;

    public event Action<PieceID, bool> OnToggleChanged;
    private bool _isSelected = false;
    private PieceID _currentId;


    // Presenter 會呼叫這個方法來填入資料
    public void Setup(RewardUIData data)
    {
        _currentId = data.Id;
        _isSelected = false;

        if (toggle != null)
        {
            toggle.SetIsOnWithoutNotify(false);
        }

        nameText.text = data.DisplayName;
        iconImage.sprite = data.Icon;
        brokenOverlay.SetActive(data.IsBroken);
        selectionOverlay.SetActive(false);
    }

    public void OnToggleValueChanged(bool isOn)
    {
        _isSelected = isOn; // 切換狀態
        selectionOverlay.SetActive(_isSelected);
        OnToggleChanged?.Invoke(_currentId, _isSelected);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
