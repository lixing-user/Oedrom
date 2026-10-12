using System;
using System.Collections.Generic;
using UnityEngine;

public class GamePhaseFacade
{
    private const int DISTANCE_WEIGHT_MULTIPLIER = 5;

    private readonly ITurnManager _turnManager;

    private readonly ICardInputManager _cardInputManager;
    private readonly IBoardInputHandler _boardInputHandler;

    private readonly IBoardQueryService _boardQuery;
    private readonly ITerrainRepository _terrainRepository;
    private readonly ITerrainMoveRule _terrainMoveRule;

    private readonly IHandService _handService;
    private readonly IRandomEventManager _eventManager;
    private readonly IResourceManager _resourceManager;

    public GamePhaseFacade(
        ITurnManager turnManager,
        IResourceManager resourceManager,
        ICardInputManager cardInputManager,      
        IBoardInputHandler boardInputHandler,
        IBoardQueryService boardQuery,
        IHandService handService,
        IRandomEventManager eventManager,
        ITerrainRepository terrainRepository,
        ITerrainMoveRule terrainMoveRule)
    {
        _turnManager = turnManager;
        _resourceManager = resourceManager;
        _cardInputManager = cardInputManager;
        _boardInputHandler = boardInputHandler;
        _terrainRepository = terrainRepository;
        _terrainMoveRule = terrainMoveRule;
        _boardQuery = boardQuery;
        _handService = handService;
        _eventManager = eventManager;
    }


    #region Input Control
    /// <summary>
    /// 關閉所有輸入（AI回合、暫停）
    /// </summary>
    public void DisableAllInput()
    {
        _cardInputManager.SetCardInputEnabled(false);
        _boardInputHandler.SetBoardInputEnabled(false);

        _cardInputManager.ForceReset();
        _boardInputHandler.ForceReset();

        Debug.Log("[Facade] 所有輸入已關閉");
    }

    /// <summary>
    /// 開啟所有輸入（玩家回合開始）
    /// </summary>
    public void EnableAllInput()
    {
        _cardInputManager.SetCardInputEnabled(true);
        _boardInputHandler.SetBoardInputEnabled(true);
        Debug.Log("[Facade] 所有輸入已開啟");
    }

    // 卡牌輸入控制
    public void EnableCardInput()
    {
        _cardInputManager.SetCardInputEnabled(true);
        Debug.Log("[Facade] 卡牌輸入已開啟");
    }

    public void DisableCardInput()
    {
        _cardInputManager.SetCardInputEnabled(false);
        _cardInputManager.ForceReset();
        Debug.Log("[Facade] 卡牌輸入已關閉");
    }

    // 棋盤輸入控制
    public void EnableBoardInput()
    {
        _boardInputHandler.SetBoardInputEnabled(true);
        Debug.Log("[Facade] 棋盤輸入已開啟");
    }

    public void DisableBoardInput()
    {
        _boardInputHandler.SetBoardInputEnabled(false);
        _boardInputHandler.ForceReset();
        Debug.Log("[Facade] 棋盤輸入已關閉");
    }
    #endregion

    #region StandbyPhase 補給增加
    public void ReplenishResources(Action onComplete)
    {
        Debug.Log("[GamePhaseFacade] 開始補給資源...");

        try
        {
            // 調用 ResourceManager 的補給方法
            if (_resourceManager is IResourceManager resourceService)
            {
                resourceService.ApplyTurnStartEffects();
                Debug.Log("[GamePhaseFacade] 補給資源成功");
            }
            else
            {
                Debug.LogWarning("[GamePhaseFacade] ResourceManager 類型不符");
            }

            onComplete?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GamePhaseFacade] 補給資源失敗: {ex.Message}");
            onComplete?.Invoke(); // 容錯處理
        }
    }
    #endregion

    #region EndPhase 補給扣除
    public void CalculateAndDeductSupply(
      List<PieceMovementRecord> movementRecords,
      Action<int> onComplete)
    {
        Debug.Log("[Facade] 開始計算補給消耗...");
        //單次消耗 = ( 距離 * 總棋子數 ) * 地形加權

        try
        {
            float totalSupplyCost = 0f;
            Team currentTeam = _turnManager.CurrentTurn;

            // 1. [陣型領域] 取得己方總棋子數
            int totalPieceCount = _boardQuery.GetPieceCountForTeam(currentTeam); 
            Debug.Log($"[Facade] {currentTeam} 方棋子總數: {totalPieceCount}");

            foreach (var record in movementRecords)
            {
                var piece = _boardQuery.GetPieceById(record.PieceId);
                Vector2Int toPos = record.ToPosition;

                // 2. [空間領域] 計算距離
                int distanceFromBottom = CalculateDistanceFromBottom(
                    toPos,
                    currentTeam,
                    _boardQuery.GetBoardHeight());

                // 算式前半段：基礎消耗
                int weightedDistance = distanceFromBottom * DISTANCE_WEIGHT_MULTIPLIER;

                // 算式前半段：基礎消耗 (加權後的距離 * 總棋子數)
                float baseCost = weightedDistance * totalPieceCount;

                PiecePosition targetPiecePos = new PiecePosition(toPos.x, toPos.y);

                // 3. [地形領域] 取得地形種類與加權係數
                var targetTerrain = _terrainRepository.GetTerrainAt(targetPiecePos);
                float terrainMultiplier = _terrainMoveRule.GetMovementCost(targetTerrain, piece.Type);

                // 4. 合併計算：基礎消耗 * 地形加權
                float finalMoveCost = baseCost * terrainMultiplier;
                totalSupplyCost += finalMoveCost;

                Debug.Log($"[Facade] 棋子 {record.PieceId} 移動至 {targetTerrain} (加權 {terrainMultiplier}x)。" +
                      $"距離:{distanceFromBottom} * 數量:{totalPieceCount} * 加權 = {finalMoveCost}");
            }

            // 5. 最終結算：四捨五入轉換為整數
            int finalCostInt = Mathf.RoundToInt(totalSupplyCost);
            Debug.Log($"[Facade] 總補給消耗: {finalCostInt}");

            bool success = _resourceManager.DeductSupply(finalCostInt);            // 通過資源服務扣除
            if (!success)
            {
                Debug.LogWarning("[Facade] 補給不足！");
            }

            onComplete?.Invoke(finalCostInt);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Facade] 補給扣除失敗: {ex.Message}");
            onComplete?.Invoke(0);
        }
    }

    private int CalculateDistanceFromBottom(
      Vector2Int position,
      Team team,
      int boardHeight)
    {
        if (team == Team.White)
        {
            return position.y;
        }
        else
        {
            return boardHeight - 1 - position.y;
        }
    }
    #endregion

    #region RandomEvent 觸發&結算
    /// <summary>
    /// Step 1: 供 MainSecPhase 呼叫。
    /// 負責抽籤、通知 UI 顯示，並回傳抽到的事件資料讓 Model 儲存
    /// </summary>
    public RandomEventData RollRandomEvent(PieceID id) => _eventManager.DraftEvent(id);
    public void ShowRandomEventUI(RandomEventData e) => _eventManager.ShowEvent(e);

    /// <summary>
    /// Step 3: 供 ResolutionPhase 呼叫。
    /// 負責真正執行事件對資源或特定棋子的影響
    /// </summary>
    public void ResolveRandomEvent(RandomEventData eventData, int selectedOptionIndex, PieceID targetPieceId)
    {
        if (eventData == null) return;

        Debug.Log($"[Facade] 開始結算隨機事件效果: {eventData.eventName}");

        _eventManager.ResolvePendingEvent(eventData, selectedOptionIndex, targetPieceId);
    }

    public void TriggerTerrainEvent(TerrainType terrain)
    {
        _eventManager.TriggerTerrainEvent(terrain);
    }
    #endregion

    #region 狀態查詢
    // 查詢當前回合
    public Team GetCurrentTeam() => _turnManager.CurrentTurn;

    /// <summary>
    /// 檢查是否有棋子操作進行中
    /// </summary>
    public bool IsPieceCurrentlySelected()
    {
        return _boardInputHandler.HasActiveBoardInput;
    }

    /// <summary>
    /// 檢查是否有卡牌操作進行中
    /// </summary>
    public bool IsCardCurrentlySelected()
    {
        return _cardInputManager.HasActiveCardInput;
    }

    /// <summary>
    /// 檢查是否有任何輸入操作進行中
    /// </summary>
    public bool HasAnyActiveInput()
    {
        return _cardInputManager.HasActiveCardInput ||
               _boardInputHandler.HasActiveBoardInput;
    }

    /// <summary>
    /// 獲取當前手牌數量
    /// </summary>
    public int GetCurrentHandSize()
    {
        return _handService.GetHandCards().Count;
    }

    /// <summary>
    /// 獲取手牌上限
    /// </summary>
    public int GetMaxHandSize()
    {
        return _handService.GetMaxHandSize();
    }
    #endregion
}
