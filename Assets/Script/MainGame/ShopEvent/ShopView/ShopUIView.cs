using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopUIView : MonoBehaviour
{
    [Header("BuyWindows")]
    [SerializeField] private PanelView ItemInfoPanel;
    [SerializeField] private ButtonView CloseInfoButton;
    [SerializeField] private ButtonView BuyButton;

    [Header("SellWindows")]
    [SerializeField] private PanelView SellObject;
    [SerializeField] private TextMeshProUGUI TotalSellValueText;
    [SerializeField] private ButtonView SellRequestButon;
    [SerializeField] private PanelView SellConfirmPanel;
    [SerializeField] private ButtonView SellButon;

    [Header("RepairWindows")]
    [SerializeField] private PanelView RepairObject;
    [SerializeField] private TextMeshProUGUI TotalRepairValueText;
    [SerializeField] private ButtonView RepairRequestButton;
    [SerializeField] private PanelView RepairConfirmPanel;
    [SerializeField] private ButtonView RepairButton;

    [Header("SectionChangeButton")]
    [SerializeField] private ButtonView LChangeSectionButton;
    [SerializeField] private ButtonView RChangeSectionButton;

    [Header("Loot")]
    [SerializeField] private PanelView LootConfirmPanel;
    [SerializeField] private ButtonView LootConfirmButton;

    [Header("Exit")]
    //[SerializeField] private ButtonView ExitShopRequestButton;
    [SerializeField] private ButtonView ExitShopConfirmButton;
    //[SerializeField] private PanelView ExitShopPanel;

    public event System.Action OnConfirmBuyClicked;

    public event System.Action OnRequestSellClicked;
    public event System.Action OnConfirmSellClicked;

    public event System.Action OnRequestedRepairClicked;
    public event System.Action OnConfirmRepairClicked;

    public event System.Action OnSectionChangeLeftRequested;
    public event System.Action OnSectionChangeRightRequested;

    public event System.Action OnLootConfirmed;

    public event System.Action OnExitShopConfirmed;
    public event System.Action OnBackClicked;


    void Start()
    {
        LChangeSectionButton.OnClicked += () => OnSectionChangeLeftRequested?.Invoke();
        RChangeSectionButton.OnClicked += () => OnSectionChangeRightRequested?.Invoke();

        BuyButton.OnClicked += () => OnConfirmBuyClicked?.Invoke();
        CloseInfoButton.OnClicked += () => OnBackClicked?.Invoke();

        SellRequestButon.OnClicked += () => OnRequestSellClicked?.Invoke();
        SellButon.OnClicked += () => OnConfirmSellClicked?.Invoke();
        SellConfirmPanel.OnBackRequested += () => OnBackClicked?.Invoke();

        RepairRequestButton.OnClicked += () => OnRequestedRepairClicked?.Invoke();
        RepairButton.OnClicked += () => OnConfirmRepairClicked?.Invoke();
        RepairConfirmPanel.OnBackRequested += () => OnBackClicked?.Invoke();

        LootConfirmButton.OnClicked += () => OnLootConfirmed?.Invoke();
        LootConfirmPanel.OnBackRequested += () => OnBackClicked?.Invoke();

        ExitShopConfirmButton.OnClicked += () => OnExitShopConfirmed?.Invoke();
    }
    void OnDestroy()
    {
        BuyButton.OnClicked -= () => OnConfirmBuyClicked?.Invoke();
        CloseInfoButton.OnClicked -= () => OnBackClicked?.Invoke();

        SellRequestButon.OnClicked -= () => OnRequestSellClicked?.Invoke();
        SellButon.OnClicked -= () => OnConfirmSellClicked?.Invoke();
        SellConfirmPanel.OnBackRequested -= () => OnBackClicked?.Invoke();

        RepairRequestButton.OnClicked -= () => OnRequestedRepairClicked?.Invoke();
        RepairButton.OnClicked -= () => OnConfirmRepairClicked?.Invoke();
        RepairConfirmPanel.OnBackRequested -= () => OnBackClicked?.Invoke();

        LootConfirmButton.OnClicked -= () => OnLootConfirmed?.Invoke();
        LootConfirmPanel.OnBackRequested -= () => OnBackClicked?.Invoke();

        ExitShopConfirmButton.OnClicked -= () => OnExitShopConfirmed?.Invoke();
    }

    #region 互動內容

    #region Buy & Loot
    public void ShowItemInfoPanel()
    {
        ItemInfoPanel.Show();
        // 如果有商品名稱、價格文字的參考，可以在這裡一併更新
    }

    public void HideItemInfoPanel()
    {
        ItemInfoPanel.Hide();
    }

    public void ShowLootPanel()
    {
        LootConfirmPanel.Show();
    }

    public void HideLootPanel()
    {
        LootConfirmPanel.Hide();
    }
    #endregion

    #region Sell
    public void UpdateSellValueText(int totalValue)
    {
        if (TotalSellValueText != null)
            TotalSellValueText.text = $"Total: {totalValue} G";
    }

    public void ShowSellConfirmPanel()
    {
        TotalSellValueText.alpha = 1f; // 確保文字可見
        SellConfirmPanel.Show();
    }

    public void HideSellConfirmPanel()
    {
        TotalSellValueText.alpha = 0f; // 隱藏文字
        SellConfirmPanel.Hide();
    }

    public void ShowSellRequestButton()
    {
        SellObject.Show();
    }
    public void HideSellRequestButton()
    {
        SellObject.Hide();
    }
    #endregion

    #region Repair
    public void ShowRepairRequestButton()
    {
        RepairObject.Show();
    } 
    public void HideRepairRequestButton()
    {
        RepairObject.Hide();
    }

    public void UpdateRepairValueText(int totalValue)
    {
        if (TotalRepairValueText != null)
            TotalRepairValueText.text = $"Total: {totalValue} G";
    }

    public void ShowRepairConfirmPanel()
    {
        TotalRepairValueText.alpha = 1f; // 確保文字可見
        RepairConfirmPanel.Show();
    }
    public void HideRepairConfirmPanel()
    {
        TotalRepairValueText.alpha = 0f; // 隱藏文字
        RepairConfirmPanel.Hide();
    }
    #endregion

    #endregion
}
