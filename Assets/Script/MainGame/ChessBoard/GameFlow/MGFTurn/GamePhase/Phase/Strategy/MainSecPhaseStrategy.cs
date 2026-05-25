using System.Collections;
using UnityEngine;
using Zenject;

public interface IMainSecPhaseStrategy
{
    Team TargetTeam { get; }
    IEnumerator Trigger(ResolutionType type, GamePhaseModel model);
}

public class PlayerMainSecPhaseStrategy : IMainSecPhaseStrategy
{
    public Team TargetTeam => Team.White;
    [Inject] private readonly SignalBus _signalBus;

    public IEnumerator Trigger(ResolutionType type, GamePhaseModel model)
    {
        Debug.Log("[PlayerMainSecPhaseStrategy] 通知 UI 顯示結算確認");
        _signalBus.Fire(new ResolutionTriggeredSignal(type));

        // 玩家策略在這裡不負責「等待」，因為 UI 會發射切換場景信號
        // 而 Phase 本身會等待場景返回
        yield return null;
    }
}

public class AIMainSecPhaseStrategy : IMainSecPhaseStrategy
{
    public Team TargetTeam => Team.Black;
    [Inject] private readonly SignalBus _signalBus;

    public IEnumerator Trigger(ResolutionType type, GamePhaseModel model)
    {
        Debug.Log("[AIMainSecPhaseStrategy] AI 直接發送場景切換請求");

        if (type == ResolutionType.Battle)
        {
            _signalBus.Fire(new RequestEnterBattleSignal
            {
                BattleType = model.PendingBattle?.BattleType ?? BattleType.Normal,
                PendingBattle = model.PendingBattle
            });
        }
        else if (type == ResolutionType.Shop)
        {
            Debug.Log("[AIStrategy] AI 踩到商店，邏輯攔截：不顯示 UI，不進入商店");
        }

        yield return new WaitForSeconds(0.5f);
    }
}