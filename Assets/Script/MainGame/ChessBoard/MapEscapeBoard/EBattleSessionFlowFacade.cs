using UnityEngine;
using Zenject;

public class EBattleSessionFlowFacade : IFlowFacade
{
    [Inject] private GameFlowStateMachine _flowMachine;

    private readonly ITurnManager _turnManager;

    private readonly IPieceSelectionExtensionRegistry _extensionRegistry;
    private readonly IBoardInputHandler _boardInputHandler;

    private readonly IBoardQueryService _boardQuery;
    private readonly IHandService _handService;

    public EBattleSessionFlowFacade(
        ITurnManager turnManager,
        IPieceSelectionExtensionRegistry extensionRegistry,
        IBoardInputHandler boardInputHandler,
        IBoardQueryService boardQuery,
        IHandService handService)
    {
        _turnManager = turnManager;
        _extensionRegistry = extensionRegistry;
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
        _boardInputHandler.SetBoardInputEnabled(false);

        _boardInputHandler.ForceReset();

        Debug.Log("[Facade] 所有輸入已關閉");
    }

    /// <summary>
    /// 開啟所有輸入（玩家回合開始）
    /// </summary>
    public void EnableAllInput()
    {
        _boardInputHandler.SetBoardInputEnabled(true);
        Debug.Log("[Facade] 所有輸入已開啟");
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

    #region 狀態查詢
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

    #region Unused Interface Methods
    public void EnableCardInput() { }
    public void DisableCardInput() { }
    public void EnableStandbyInput() { }
    public void DisableStandbyInput() { }
    public void EnableBoardOnlyInput() { }
    public void EnableStandbyOnlyInput() { }
    public int GetCurrentHandSize() => 0;
    public int GetMaxHandSize() => 0;
    #endregion
}
