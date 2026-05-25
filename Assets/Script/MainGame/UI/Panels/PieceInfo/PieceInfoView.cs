using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PieceInfoView : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Property")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text terrainText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image iconImage;

    [Header("Event Status List")]
    [SerializeField] private Transform _statusContainer; // 指向 ScrollView 裡面的 Content
    [SerializeField] private StatusItemView _statusItemPrefab; // 單一狀態的 Prefab
    private List<StatusItemView> _activeStatusItems = new List<StatusItemView>();


    public event Action OnRightClickRequested;

    void Awake() 
    { 
        Hide();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && _canvasGroup.alpha > 0)
        {
            OnRightClickRequested?.Invoke();
        }
    }

    public void Show()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }


    public void UpdatePieceInfo(
        string pieceName,
        Sprite icon,
        string position,
        string state,
        string terrain)
    {
        nameText.text = pieceName;
        iconImage.sprite = icon;
        positionText.text = $"位置：{position}";
        statusText.text = $"狀態：{state}";
        terrainText.text = $"地形：{terrain}";
    }


    public void UpdateStatuses(List<EventStatusUIData> statuses)
    {
        // 1. 清空舊的 UI 節點
        foreach (Transform child in _statusContainer)
        {
            Destroy(child.gameObject);
        }
        _activeStatusItems.Clear();

        // 2. 生成新的 UI 節點
        foreach (var statusData in statuses)
        {
            var newItem = Instantiate(_statusItemPrefab, _statusContainer);
            newItem.Setup(statusData);
            _activeStatusItems.Add(newItem);
        }

        Debug.Log($"[PieceInfoView] UI 節點生成完畢，目前 Container 內有 {_activeStatusItems.Count} 個狀態。");
    }
}