using UnityEngine;
using Zenject;

public interface IGameFlowStateFactory
{
    IGameFlowState CreateSetupState(GameFlowStateMachine machine, DiContainer container);
    IGameFlowState CreatePlayerTurnState(GameFlowStateMachine machine, DiContainer container);
    IGameFlowState CreateEnemyTurnState(GameFlowStateMachine machine, DiContainer container);
    IGameFlowState CreateResolutionState(GameFlowStateMachine machine, DiContainer container);
    IGameFlowState CreateEndGameState(GameFlowStateMachine machine, DiContainer container);
}


#region MainGameField
public class MainBattleStateFactory : IGameFlowStateFactory
{
    public IGameFlowState CreateSetupState(GameFlowStateMachine machine, DiContainer container)
    {
        return new SetUpState(machine, container);  
    }

    public IGameFlowState CreatePlayerTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new PlayerTurnState(machine, container); 
    }

    public IGameFlowState CreateEnemyTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new EnemyTurnState(machine, container);  
    }

    public IGameFlowState CreateResolutionState(GameFlowStateMachine machine, DiContainer container)
    {
        return new ResolutionFlowState(machine, container);  
    }

    public IGameFlowState CreateEndGameState(GameFlowStateMachine machine, DiContainer container)
    {
        return new GameEndState(machine, container);  
    }
}
#endregion

#region BattleSession
public class SimplifiedBattleStateFactory : IGameFlowStateFactory
{
    public IGameFlowState CreateSetupState(GameFlowStateMachine machine, DiContainer container)
    {
        return new SetUpStateS(machine, container); 
    }

    public IGameFlowState CreatePlayerTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new PlayerTurnStateS(machine, container);  
    }

    public IGameFlowState CreateEnemyTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new EnemyTurnStateS(machine, container);  
    }

    public IGameFlowState CreateResolutionState(GameFlowStateMachine machine, DiContainer container)
    {
        return new ResolutionFlowStateS(machine, container); 
    }

    public IGameFlowState CreateEndGameState(GameFlowStateMachine machine, DiContainer container)
    {
        return new GameEndStateS(machine, container);  
    }
}
#endregion

#region ChaseBattle
public class ChaseBattleFlowStateFactory : IGameFlowStateFactory
{
    public IGameFlowState CreateSetupState(GameFlowStateMachine machine, DiContainer container)
    {
        return new SetUpStateN(machine, container);
    }

    public IGameFlowState CreatePlayerTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new PlayerTurnStateN(machine, container);
    }

    public IGameFlowState CreateEnemyTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new EnemyTurnStateN(machine, container);
    }

    public IGameFlowState CreateResolutionState(GameFlowStateMachine machine, DiContainer container)
    {
        return new ResolutionFlowStateN(machine, container);
    }

    public IGameFlowState CreateEndGameState(GameFlowStateMachine machine, DiContainer container)
    {
        return new GameEndStateN(machine, container);
    }
}
#endregion

#region EscapeBattle
public class EscapeBattleFlowStateFactory : IGameFlowStateFactory
{
    public IGameFlowState CreateSetupState(GameFlowStateMachine machine, DiContainer container)
    {
        return new SetUpStateE(machine, container);
    }

    public IGameFlowState CreatePlayerTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new PlayerTurnStateE(machine, container);
    }

    public IGameFlowState CreateEnemyTurnState(GameFlowStateMachine machine, DiContainer container)
    {
        return new EnemyTurnStateE(machine, container);
    }

    public IGameFlowState CreateResolutionState(GameFlowStateMachine machine, DiContainer container)
    {
        return new ResolutionFlowStateE(machine, container);
    }

    public IGameFlowState CreateEndGameState(GameFlowStateMachine machine, DiContainer container)
    {
        return new GameEndStateE(machine, container);
    }
}
#endregion