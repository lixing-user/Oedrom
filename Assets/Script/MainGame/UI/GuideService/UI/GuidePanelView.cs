using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GuidePanelView : MonoBehaviour
{
    [Header("Tab Controls")]
    [SerializeField] private PanelView GuidePanel;
    [SerializeField] private CanvasGroup GuidePanelCanvasGroup; // 用於控制整個標籤區的互動性和透明度

    [SerializeField] private Transform tabContainer;
    [SerializeField] private GameObject tabButtonPrefab; // 內含 ButtonView
    private List<ButtonView> _tabButtons = new List<ButtonView>();

    [Header("Content Display")]
    [SerializeField] private TextMeshProUGUI pageTitleText;
    [SerializeField] private TextMeshProUGUI pageDescriptionText;
    [SerializeField] private Image contentImage;

    [Header("Pagination Controls")]
    [SerializeField] private ButtonView prevPageButton;
    [SerializeField] private ButtonView nextPageButton;
    [SerializeField] private TextMeshProUGUI pageIndicatorText; // 例如 "1 / 3"

    public event Action<int> OnTabClicked;
    public event Action OnPrevClicked;
    public event Action OnNextClicked;
    public event Action OnBackRequested;


    private void OnEnable()
    {
        if (prevPageButton != null) prevPageButton.OnClicked += () => OnPrevClicked?.Invoke();
        if (nextPageButton != null) nextPageButton.OnClicked += () => OnNextClicked?.Invoke();

        if (GuidePanel != null) GuidePanel.OnBackRequested += HandleBackRequested;
    }

    private void OnDisable()
    {
        if (prevPageButton != null) prevPageButton.ClearListeners();
        if (nextPageButton != null) nextPageButton.ClearListeners();

        if (GuidePanel != null) GuidePanel.OnBackRequested -= HandleBackRequested;
    }
    
    public void Show()
    {
        GuidePanelCanvasGroup.alpha = 1;
        GuidePanelCanvasGroup.interactable = true;
        GuidePanelCanvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        GuidePanelCanvasGroup.alpha = 0;
        GuidePanelCanvasGroup.interactable = false;
        GuidePanelCanvasGroup.blocksRaycasts = false;
    }


    private void HandleBackRequested()
    {
        OnBackRequested?.Invoke();
    }


    public void InitializeTabs(List<GuideTabDisplayData> tabDatas)
    {
        foreach (var btn in _tabButtons) Destroy(btn.gameObject);
        _tabButtons.Clear();

        for (int i = 0; i < tabDatas.Count; i++)
        {
            int index = i;
            var data = tabDatas[i];

            var btnObj = Instantiate(tabButtonPrefab, tabContainer);
            var btnView = btnObj.GetComponent<ButtonView>();

            btnView.SetContent(data.Name, "", data.Icon);
            btnView.OnClicked += () => OnTabClicked?.Invoke(index);
            _tabButtons.Add(btnView);
        }
    }


    public void UpdatePageContent(GuidePageData page, int currentPageIndex, int totalPages)
    {
        pageTitleText.text = page.title;
        pageDescriptionText.text = page.description;
        contentImage.sprite = page.illustration;
        contentImage.gameObject.SetActive(page.illustration != null);
        pageIndicatorText.text = $"{currentPageIndex + 1} / {totalPages}";

        prevPageButton.gameObject.SetActive(currentPageIndex > 0);
        nextPageButton.gameObject.SetActive(currentPageIndex < totalPages - 1);
    }


    public void HighlightTab(int activeIndex)
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            // 透過 CanvasGroup 控制未選中標籤的透明度
            var cg = _tabButtons[i].GetComponent<CanvasGroup>() ?? _tabButtons[i].gameObject.AddComponent<CanvasGroup>();
            cg.alpha = (i == activeIndex) ? 1.0f : 0.5f;
        }
    }
}
