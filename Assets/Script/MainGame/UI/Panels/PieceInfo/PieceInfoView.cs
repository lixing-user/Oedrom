using DG.Tweening;
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
    [SerializeField] private RectTransform _panelRect;

    [Header("Property")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text terrainText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image iconImage;

    [Header("Event Status List")]
    [SerializeField] private Transform _statusContainer; // 指向 ScrollView 裡面的 Content
    [SerializeField] private StatusItemView _statusItemPrefab; // 單一狀態的 Prefab


    [Header("Animation Settings")]
    [SerializeField] private float _transitionDuration = 0.3f;
    [SerializeField] private Ease _transitionEase = Ease.OutCubic;
    [Header("Position Settings")]
    [SerializeField] private Vector2 _standardPos = Vector2.zero;
    [SerializeField] private Vector2 _subtlePos = new Vector2(-200, 0);


    private List<StatusItemView> _activeStatusItems = new List<StatusItemView>();

    private Sequence _activeSequence;

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
    void OnDestroy()
    {
        if (_activeSequence != null)
        {
            _activeSequence.Kill();
            _activeSequence = null;
        }
    }

    public void Show()
    {
        //if (_canvasGroup != null)
        //{
        //    _canvasGroup.alpha = 1f;
        //    _canvasGroup.interactable = true;
        //    _canvasGroup.blocksRaycasts = true;
        //}
        if (_canvasGroup == null || _panelRect == null) return;

        // 啟用互動
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _activeSequence?.Kill();
        _activeSequence = DOTween.Sequence();

        // 透明度漸變到 1，使用 _transitionDuration 作為速度（時間）
        _activeSequence.Join(_canvasGroup.DOFade(1f, _transitionDuration));

        // 位置滑動到 _standardPos
        _activeSequence.Join(_panelRect.DOAnchorPos(_standardPos, _transitionDuration));

        _activeSequence.SetEase(_transitionEase);
    }
    public void Hide()
    {
        //if (_canvasGroup != null)
        //{
        //    _canvasGroup.alpha = 0f;
        //    _canvasGroup.interactable = false;
        //    _canvasGroup.blocksRaycasts = false;
        //}
        if (_canvasGroup == null || _panelRect == null) return;

        // 關閉互動，避免動畫過程中被點擊
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _activeSequence?.Kill();
        _activeSequence = DOTween.Sequence();

        // 透明度漸變到 0，使用 _transitionDuration 作為速度（時間）
        _activeSequence.Join(_canvasGroup.DOFade(0f, _transitionDuration));

        // 位置滑動到 _subtlePos
        _activeSequence.Join(_panelRect.DOAnchorPos(_subtlePos, _transitionDuration));

        _activeSequence.SetEase(_transitionEase);
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