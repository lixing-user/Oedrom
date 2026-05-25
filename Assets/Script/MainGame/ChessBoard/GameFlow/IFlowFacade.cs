using UnityEngine;

public interface IFlowFacade : IInputControlFacade, IGameQueryFacade { }

#region 職責接口
// 輸入控制職責
public interface IInputControlFacade
{
    void EnableAllInput();
    void DisableAllInput();
    void EnableCardInput();
    void DisableCardInput();
    void EnableBoardInput();
    void DisableBoardInput();
    void EnableStandbyInput();
    void DisableStandbyInput();
    void EnableBoardOnlyInput();
    void EnableStandbyOnlyInput();
}

// 狀態查詢職責
public interface IGameQueryFacade
{
    int GetCurrentHandSize();
    int GetMaxHandSize();
    bool HasEnemyPieceAt(Vector2Int position);
    Team GetCurrentTeam();
    GameFlow GetCurrentFlow();
}
#endregion


public class NullFlowFacade : IFlowFacade
{
    // 輸入控制接口實現（空方法）
    public void EnableAllInput() { }
    public void DisableAllInput() { }
    public void EnableCardInput() { }
    public void DisableCardInput() { }
    public void EnableBoardInput() { }
    public void DisableBoardInput() { }
    public void EnableStandbyInput() { }
    public void DisableStandbyInput() { }
    public void EnableBoardOnlyInput() { }
    public void EnableStandbyOnlyInput() { }
    // 狀態查詢接口實現（返回預設值）
    public int GetCurrentHandSize() => 0;
    public int GetMaxHandSize() => 0;
    public bool HasEnemyPieceAt(Vector2Int position) => false;
    public Team GetCurrentTeam() => Team.White;
    public GameFlow GetCurrentFlow() => GameFlow.Setup;
}