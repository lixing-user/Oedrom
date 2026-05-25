using UnityEngine;
using Zenject;

public class BattleSessionFlowFacade : IFlowFacade
{
    [Inject] private GameFlowStateMachine _flowMachine;

    private readonly ITurnManager _turnManager;

    private readonly IPieceSelectionExtensionRegistry _extensionRegistry;
    private readonly ICardInputManager _cardInputManager;
    private readonly IBoardInputHandler _boardInputHandler;

    private readonly IBoardQueryService _boardQuery;
    private readonly IHandService _handService;

    public BattleSessionFlowFacade(
        ITurnManager turnManager,
        IPieceSelectionExtensionRegistry extensionRegistry,
        ICardInputManager cardInputManager,
        IBoardInputHandler boardInputHandler,
        IBoardQueryService boardQuery,
        IHandService handService)
    {
        _turnManager = turnManager;
        _extensionRegistry = extensionRegistry;
        _cardInputManager = cardInputManager;
        _boardInputHandler = boardInputHandler;
        _boardQuery = boardQuery;
        _handService = handService;
    }


    #region Input Control
    /// <summary>
    /// 關閉所有輸入（AI回合、暫停）
    /// </summary>
    public void DisableAllInput()
    {
        _cardInputManager.SetCardInputEnabled(false);
        _boardInputHandler.SetBoardInputEnabled(false);

        // 2. 強制重置底層狀態機與緩存資料
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

    // 啟用待命區輸入
    public void EnableStandbyInput()
    {
        _extensionRegistry.EnableExtension("StandbyPieceSelection");
        Debug.Log("[Facade] 待命區輸入已開啟");
    }

    public void DisableStandbyInput()
    {
        _extensionRegistry.DisableExtension("StandbyPieceSelection");
        Debug.Log("[Facade] 待命區輸入已關閉");
    }

    // 只允許棋盤移動，禁止從待命區拖回
    public void EnableBoardOnlyInput()
    {
        _boardInputHandler.SetBoardInputEnabled(true);
        _extensionRegistry.EnableExtension("BoardPieceSelection");
        _extensionRegistry.DisableExtension("StandbyPieceSelection");  // 停用待命區
        Debug.Log("[Facade] 棋盤輸入已開啟（不含待命區）");
    }

    // 只允許從待命區拖回，禁止棋盤移動
    public void EnableStandbyOnlyInput()
    {
        _boardInputHandler.SetBoardInputEnabled(true);
        _extensionRegistry.DisableExtension("BoardPieceSelection");  // 停用棋盤
        _extensionRegistry.EnableExtension("StandbyPieceSelection");
        Debug.Log("[Facade] 待命區輸入已開啟（不含棋盤移動）");
    }
    #endregion

    #region 狀態查詢
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


    public bool HasEnemyPieceAt(Vector2Int position)
    {
        var piece = _boardQuery.GetPieceAt(position);

        if (piece == null)
            return false;

        // 檢查是否為敵方棋子
        Team currentTeam = GetCurrentTeam();
        return piece.Team != currentTeam;
    }

    public Team GetCurrentTeam() => _turnManager.CurrentTurn;
    public GameFlow GetCurrentFlow() => _flowMachine.CurrentFlow;
    #endregion
}
