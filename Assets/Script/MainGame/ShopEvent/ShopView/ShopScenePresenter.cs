using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public struct ShopItemDisplayData
{
    public string ItemId;
    public ShopItemType ItemType;      // 讓 Spawner 知道這是棋子還是補給品
    public ChessPieceType? PieceType;
    public string DisplayName;
    public int Cost;
}

public class ShopScenePresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IShopService _shopService;
    [Inject] private IHandUIManager _handUIManager;

    private ShopUIView _view;
    private Shop3DView _view3D;
    private ShopCameraControler _cameraView; 
    private int _currentSectionIndex = 0;

    private CardID _pendingLootCardId = CardID.Empty;
    private string _selectedItemId;

    private List<CardID> _pendingSellCardIds = new List<CardID>();
    private int _accumulatedSellValue = 0;

    private List<CardID> _pendingRepairCardIds = new List<CardID>();
    private int _accumulatedRepairValue = 0;

    #region Initialize
    public void InitializeView()
    {
        _view = GetComponent<ShopUIView>();
        _view3D = GetComponent<Shop3DView>();
        _cameraView = GetComponent<ShopCameraControler>();

        _handUIManager.RefreshAllCards();
        BindViewEvents();

        if (_shopService.IsShopLooted())
        {
            SetupLootedShopView();
        }
        else
        {
            InitializeShopItems(); // 正常的生成商品
        }

        _view.HideSellRequestButton();  
        _view.HideRepairRequestButton();
        HandleCloseAllPanel(); // 確保一開始所有面板都是關閉的
    }
    void OnDestroy()
    {
        UnbindViewEvent();
    }

    private void BindViewEvents()
    {
        _view.OnSectionChangeLeftRequested += HandleSectionChangeLeft;
        _view.OnSectionChangeRightRequested += HandleSectionChangeRight;
        _view.OnConfirmBuyClicked += HandleBuyItemConfirmed;
        _view.OnRequestSellClicked += HandleSellItemRequested;
        _view.OnConfirmSellClicked += HandleSellItemConfirmed;
        _view.OnRequestedRepairClicked += HandleRepairItemRequested;
        _view.OnConfirmRepairClicked += HandleRepairItemConfirmed;
        _view.OnLootConfirmed += HandleLootConfirmed;
        _view.OnBackClicked += HandleCloseAllPanel;
        _view.OnExitShopConfirmed += HandleShopExit;

        _signalBus.Subscribe<ShopItemClickedSignal>(OnShopItemClicked);
        _signalBus.Subscribe<RequestSectionChangeSignal>(OnSectionChangeRequested);
        _signalBus.Subscribe<RequestLootBattleSignal>(OnLootConfirmRequested);
        _signalBus.Subscribe<RequestAddSellItemSignal>(OnAddSellItemRequested);
        _signalBus.Subscribe<RequestAddRepairItemSignal>(OnAddRepairItemRequested);
    }
    private void UnbindViewEvent()
    {
        _view.OnSectionChangeLeftRequested -= HandleSectionChangeLeft;
        _view.OnSectionChangeRightRequested -= HandleSectionChangeRight;
        _view.OnConfirmBuyClicked -= HandleBuyItemConfirmed;
        _view.OnRequestSellClicked -= HandleSellItemRequested;
        _view.OnConfirmSellClicked -= HandleSellItemConfirmed;
        _view.OnRequestedRepairClicked -= HandleRepairItemRequested;
        _view.OnConfirmRepairClicked -= HandleRepairItemConfirmed;
        _view.OnLootConfirmed -= HandleLootConfirmed;
        _view.OnBackClicked -= HandleCloseAllPanel;
        _view.OnExitShopConfirmed -= HandleShopExit;

        _signalBus.TryUnsubscribe<ShopItemClickedSignal>(OnShopItemClicked);
        _signalBus.TryUnsubscribe<RequestSectionChangeSignal>(OnSectionChangeRequested);
        _signalBus.TryUnsubscribe<RequestLootBattleSignal>(OnLootConfirmRequested);
        _signalBus.TryUnsubscribe<RequestAddSellItemSignal>(OnAddSellItemRequested);
        _signalBus.TryUnsubscribe<RequestAddRepairItemSignal>(OnAddRepairItemRequested);
    }


    private void InitializeShopItems()
    {
        var availableItems = _shopService.GetAvailableShopItems();

        var displayDataList = new List<ShopItemDisplayData>();

        foreach (var item in availableItems)
        {
            // 3. 打包成 View Model
            displayDataList.Add(new ShopItemDisplayData
            {
                ItemId = item.Id,
                ItemType = item.ItemType,
                PieceType = item.PieceType,
                DisplayName = item.PieceType.ToString(), 
                Cost = item.Cost
            });
        }
        // 4. 把組裝好的資料丟給 View 顯示
        _view3D.Spawn3DItems(displayDataList);
    }

    private void SetupLootedShopView()
    {
        Debug.Log("[ShopPresenter] 這間店已經被搶過了！");
    }
    #endregion

    #region 購買邏輯
    private void OnShopItemClicked(ShopItemClickedSignal signal)
    {
        _selectedItemId = signal.ItemId;
        // 在這裡可以進一步從 Domain 抓取詳細資料丟給 View 顯示名稱與價格
        _view.ShowItemInfoPanel();
        Debug.Log($"點擊了商品: {signal.ItemId}");
    }

    private void HandleBuyItemConfirmed()
    {
        if (string.IsNullOrEmpty(_selectedItemId)) return;

        bool isSuccess = _shopService.TryBuyItem(_selectedItemId);

        if (isSuccess)
        {
            Debug.Log($"購買成功: {_selectedItemId}");
            _view.HideItemInfoPanel();
            _view3D.Remove3DItem(_selectedItemId);
            _selectedItemId = null; // 清除選取狀態
        }
        else
        {
            Debug.Log("購買失敗 (餘額不足或商品已不存在)");
            // 可以在此發送信號或呼叫 View 顯示錯誤提示
        }
    }
    #endregion

    #region 賣出邏輯
    private void OnAddSellItemRequested(RequestAddSellItemSignal signal)
    {
        // 避免重複加入同一張卡
        if (_pendingSellCardIds.Contains(signal.CardId)) return;

        // 1. 記錄狀態
        _pendingSellCardIds.Add(signal.CardId);

        // 2. 計算並累加金額
        int value = _shopService.GetCardSellValue(signal.CardId);
        _accumulatedSellValue += value;

        // 3. 更新 3D 畫面 (隨機放置)
        _view3D.SpawnPendingSellItem(signal.PieceType);

        // 4. 更新 UI 畫面
        _view.UpdateSellValueText(_accumulatedSellValue);
        _view.ShowSellRequestButton();

        Debug.Log($"[ShopPresenter] 準備賣出 {signal.CardId}, 目前累積金額: {_accumulatedSellValue}");
    }

    private void HandleSellItemRequested()
    {
        _view.ShowSellConfirmPanel();
    }

    private void HandleSellItemConfirmed()
    {
        if (_pendingSellCardIds.Count == 0) return;

        bool isSuccess = _shopService.TrySellCards(_pendingSellCardIds);

        if (isSuccess)
        {
            // 清理暫存狀態
            ClearPendingSellState();
        }
        else
        {
            Debug.LogWarning("[ShopPresenter] 批次賣出失敗！");
        }

        _view.HideSellRequestButton();
    }

    private void ClearPendingSellState()
    {
        _pendingSellCardIds.Clear();
        _accumulatedSellValue = 0;
        _view3D.ClearPendingSellItems();
        _view.HideSellConfirmPanel();
    }

    #region 修復邏輯
    private void OnAddRepairItemRequested(RequestAddRepairItemSignal signal)
    {
        // 避免重複加入同一張卡
        if (_pendingRepairCardIds.Contains(signal.CardId)) return;

        // 1. 記錄狀態
        _pendingRepairCardIds.Add(signal.CardId);

        // 2. 計算並累加金額
        int value = _shopService.GetCardRepairCost(signal.CardId);
        _accumulatedRepairValue += value;

        // 3. 更新 3D 畫面 (隨機放置)
        _view3D.SpawnPendingRepairItem(signal.PieceType);

        // 4. 更新 UI 畫面
        _view.UpdateRepairValueText(_accumulatedRepairValue);
        _view.ShowRepairRequestButton();
        Debug.Log($"[ShopPresenter] 準備修復 {signal.CardId}, 目前累積修復費用: {_accumulatedRepairValue}");
    }

    private void HandleRepairItemRequested()
    {
        _view.ShowRepairConfirmPanel();
    }

    private void HandleRepairItemConfirmed()
    {
        if (_pendingRepairCardIds.Count == 0) return;

        bool isSuccess = _shopService.TryRepairCard(_pendingRepairCardIds);

        if (isSuccess)
        {
            // 清理暫存狀態
            ClearPendingRepairState();
        }
        else
        {
            Debug.LogWarning("[ShopPresenter] 批次修復失敗！");
        }

        _view.HideRepairRequestButton();
    }

    private void ClearPendingRepairState()
    {
        _pendingRepairCardIds.Clear();
        _accumulatedRepairValue = 0;
        _view3D.ClearPendingRepairItems();
        _view.HideRepairConfirmPanel();
    }
    #endregion

    #endregion

    #region 切換區域
    private void OnSectionChangeRequested(RequestSectionChangeSignal signal)
    {
        ChangeSection(signal.Direction);
    }
    private void ChangeSection(int direction)
    {
        // 循環演算法： (當前 + 位移 + 總數) % 總數
        _currentSectionIndex = (_currentSectionIndex + direction + 3) % 3;
        ShopSection targetSection = (ShopSection)_currentSectionIndex;

        _cameraView.MoveToSection(targetSection);

        Debug.Log($"目前商店區域：{targetSection}");
    }

    private void HandleSectionChangeLeft()
    {
        ChangeSection(-1);
    }

    private void HandleSectionChangeRight()
    {
        ChangeSection(1);
    }
    #endregion

    #region 搶劫戰鬥邏輯
    private void OnLootConfirmRequested(RequestLootBattleSignal signal)
    {
        if (_shopService.IsShopLooted())
        {
            Debug.LogWarning("[ShopPresenter] 商店已經被洗劫一空，無法再次搶劫！");
            return;
        }

        _pendingLootCardId = signal.CardId;
        _view.ShowLootPanel();
    }

    private void HandleLootConfirmed()
    {
        if (_pendingLootCardId != CardID.Empty)
        {
            _shopService.TryTriggerLootBattle(_pendingLootCardId);
        }

        // 關閉 UI 並清除狀態
        _pendingLootCardId = CardID.Empty;
        _view.HideLootPanel();
    }

    private void HandleLootCanceled()
    {
        _pendingLootCardId = CardID.Empty;
        _view.HideLootPanel();
    }
    #endregion

    #region 離開商店
    private void HandleShopExit()
    {
        Debug.Log("[ShopPresenter] 玩家選擇離開商店");

        _shopService.ClearShopState();

        _signalBus.Fire(new RequestSessionStateChangeSignal
        {
            TargetState = GameSessionState.MainGame
        });
        //_signalBus.Fire(new SceneReactivatedSignal("Shop"));
    }
    #endregion

    private void HandleCloseAllPanel()
    {
        _selectedItemId = null;
        _view.HideLootPanel();
        _view.HideSellConfirmPanel();
        _view.HideItemInfoPanel();
    }
}

#region Signals
public class RequestSectionChangeSignal
{
    public int Direction; // 1 為右, -1 為左
}

public class ShopSectionChangedSignal
{
    public ShopSection CurrentSection;
}
#endregion