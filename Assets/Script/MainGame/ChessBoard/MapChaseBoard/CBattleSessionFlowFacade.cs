using UnityEngine;
using Zenject;

public class CBattleSessionFlowFacade : IFlowFacade
{
    [Inject] private GameFlowStateMachine _flowMachine;

    private readonly ITurnManager _turnManager;

    private readonly IPieceSelectionExtensionRegistry _extensionRegistry;
    private readonly IBoardInputHandler _boardInputHandler;

    private readonly IBoardQueryService _boardQuery;

    public void EnableAllInput()
    {

    }
    public void DisableAllInput()
    {

    }

    public void EnableBoardInput()
    {

    }
    public void DisableBoardInput()
    {

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
