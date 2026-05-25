using System.Collections.Generic;
using UnityEngine;

public enum EventEffectTargetType
{
    PlayerResource, // 作用於玩家的數值
    TriggeringPiece, // 作用於觸發事件的棋子
    RandomAllyPiece  // 作用於隨機友軍
}

#region Resource Effect

#region Supply Effect
// Domain/Effects/SupplyBoostEffect.cs
/// <summary>
/// 補給增益效果（Domain 業務邏輯）
/// </summary>
public class SupplyBoostEffect : IEventEffect
{
    public string EffectId => "supply_boost";

    private readonly float _flatAmount;
    private readonly float _percentAmount;
    private readonly int _duration;

    public SupplyBoostEffect(float flatAmount, float percentAmount, int duration)
    {
        _flatAmount = flatAmount;
        _percentAmount = percentAmount;
        _duration = duration;
    }

    public bool CanExecute(IEffectContext context)
    {
        // 檢查前置條件（例如：資源未達上限）
        return context.PlayerResource.Supply < PlayerResource.MaxSupply;
    }

    public void Execute(IEffectContext context)
    {
        // 業務邏輯：創建修改器並套用
        var modifier = new ResourceModifier(
            effectId: EffectId,
            targetResource: VariableType.supply,
            flatAmount: _flatAmount,
            percentAmount: _percentAmount,
            duration: _duration);

        context.PlayerResource.ApplyModifier(modifier);

        Debug.Log($"[SupplyBoostEffect] 套用補給增益：+{_flatAmount} / +{_percentAmount * 100}%，持續 {_duration} 回合");
    }
}


// Domain/Effects/SupplyChangeEffect.cs
public class SupplyChangeEffect : IEventEffect
{
    public string EffectId => "supply_change";
    private readonly float _amount;
    public SupplyChangeEffect(float amount)
    {
        _amount = amount;
    }
    public bool CanExecute(IEffectContext context) => true;
    public void Execute(IEffectContext context)
    {
        context.PlayerResource.AdjustSupply(_amount);
        Debug.Log($"[SupplyChangeEffect] 補給變化：{(_amount >= 0 ? "+" : "")}{_amount}");
    }
}
#endregion

#region Morale Effect
// Domain/Effects/MoraleChangeEffect.cs
public class MoraleChangeEffect : IEventEffect
{
    public string EffectId => "morale_change";

    private readonly float _amount;

    public MoraleChangeEffect(float amount)
    {
        _amount = amount;
    }

    public bool CanExecute(IEffectContext context) => true;

    public void Execute(IEffectContext context)
    {
        context.PlayerResource.AdjustMorale(_amount);
        Debug.Log($"[MoraleChangeEffect] 士氣變化：{(_amount >= 0 ? "+" : "")}{_amount}");
    }
}
#endregion

#region Popularity Effect
// Domain/Effects/PopularityChangeEffect.cs
public class PopularityChangeEffect : IEventEffect
{
    public string EffectId => "popularity_change";

    private readonly float _amount;

    public PopularityChangeEffect(float amount)
    {
        _amount = amount;
    }

    public bool CanExecute(IEffectContext context) => true;

    public void Execute(IEffectContext context)
    {
        context.PlayerResource.AdjustPopularity(_amount);
        Debug.Log($"[PopularityChangeEffect] 民心變化：{(_amount >= 0 ? "+" : "")}{_amount}");
    }
}
#endregion

#region Coin Effect
public class CoinChangeEffect : IEventEffect
{
    public string EffectId => "coin_change";
    private readonly float _amount;
    public CoinChangeEffect(float amount)
    {
        _amount = amount;
    }
    public bool CanExecute(IEffectContext context) => true;
    public void Execute(IEffectContext context)
    {
        context.PlayerResource.AdjustCoin(_amount);
        Debug.Log($"[CoinChangeEffect] 金幣變化：{(_amount >= 0 ? "+" : "")}{_amount}");
    }
}
#endregion

#region Dynamic Resource Effect
// Domain/Effects/DynamicResourceChangeEffect.cs
public class DynamicResourceChangeEffect : IEventEffect
{
    public string EffectId => "dynamic_resource_change";

    private readonly VariableType _targetResource;
    private readonly VariableType _sourceResource;
    private readonly float _flatAmount;
    private readonly float _multiplier;

    public DynamicResourceChangeEffect(
        VariableType targetResource,
        VariableType sourceResource,
        float flatAmount,
        float multiplier)
    {
        _targetResource = targetResource;
        _sourceResource = sourceResource;
        _flatAmount = flatAmount;
        _multiplier = multiplier;
    }

    public bool CanExecute(IEffectContext context) => true;

    public void Execute(IEffectContext context)
    {
        // 1. 取得「來源資源」當下的數值
        float sourceValue = GetResourceValue(context.PlayerResource, _sourceResource);

        // 2. 代入公式計算最終要給予/扣除的數值
        // 公式：基礎值 + (來源值 * 加成係數)
        float finalAmount = _flatAmount + (sourceValue * _multiplier);

        // 3. 將計算結果應用到「目標資源」
        ApplyResourceChange(context.PlayerResource, _targetResource, finalAmount);

        Debug.Log($"[DynamicEffect] 讀取 {_sourceResource}({sourceValue})，給予 {_targetResource} 變化量：{finalAmount}");
    }

    // 輔助方法：讀取特定資源的數值
    private float GetResourceValue(PlayerResource resource, VariableType type)
    {
        return type switch
        {
            VariableType.morale => resource.Morale,
            VariableType.popularity => resource.Popularity,
            VariableType.supply => resource.Supply,
            VariableType.coin => resource.Coin,
            _ => 0f
        };
    }

    // 輔助方法：寫入特定資源的數值
    private void ApplyResourceChange(PlayerResource resource, VariableType type, float amount)
    {
        switch (type)
        {
            case VariableType.morale: resource.AdjustMorale(amount); break;
            case VariableType.popularity: resource.AdjustPopularity(amount); break;
            case VariableType.supply: resource.AdjustSupply(amount); break;
            case VariableType.coin: resource.AdjustCoin(amount); break;
        }
    }
}
#endregion

#endregion

#region Deck Effect
public class AddCardEffect : IEventEffect
{
    public string EffectId => "add_card_to_deck";

    private readonly ChessPieceType _type;
    private readonly Team _team;
    private readonly int _amount;

    public AddCardEffect(ChessPieceType type, Team team, int amount)
    {
        _type = type;
        _team = team;
        _amount = amount;
    }

    public bool CanExecute(IEffectContext context)
    {
        // 檢查手牌是否已滿 
        return context.PlayerHand.Cards.Count < context.PlayerHand.MaxSize;
    }

    public void Execute(IEffectContext context)
    {
        for (int i = 0; i < _amount; i++)
        {
            if (_type == ChessPieceType.None)
            {
                // 如果是 None，讓 DeckService 來隨機抽一張
                var cards = context.DeckService.DrawRandomCards(1, _team);
                if (cards != null && cards.Count > 0)
                {
                    Debug.Log($"[AddCardEffect] 隨機獲得卡牌: {cards[0].CardType}");
                }
            }
            else
            {
                // 如果有指定類型，直接呼叫 DrawCard
                var card = context.DeckService.DrawCard(_type, _team);
                if (card != null)
                {
                    Debug.Log($"[AddCardEffect] 獲得指定卡牌: {_type}");
                }
            }
        }
    }
}
#endregion

#region Trigger Battle Effects
public class TriggerBattleEffect : IEventEffect
{
    public string EffectId => "trigger_battle";
    private readonly BattleType _battleType;
    private readonly string _targetBattleId; // 企劃填寫的敵人陣容 ID

    public TriggerBattleEffect(BattleType battleType, string targetBattleId)
    {
        _battleType = battleType;
        _targetBattleId = targetBattleId;
    }

    // 優化 1：嚴格的安全檢查
    public bool CanExecute(IEffectContext context)
    {
        // 必須要有具體的棋子觸發，且該棋子確實存在於盤面上
        return context.TargetPieceId != null &&
               context.BoardQuery.GetPieceById(context.TargetPieceId) != null;
    }

    public void Execute(IEffectContext context)
    {
        // 1. 取得觸發事件的倒霉鬼 (玩家棋子)
        var triggeringPiece = context.BoardQuery.GetPieceById(context.TargetPieceId);

        // 2. 借鑒 LootBattle：組裝非對稱的 BattleSessionData
        var eventBattleData = new BattleSessionData
        {
            BattleId = BattleId.Generate().Value,
            BattleType = _battleType, // 例如 BattleType.Event

            // === 攻擊方 (玩家被捲入) ===
            AttackerId = triggeringPiece.Id,
            AttackerType = triggeringPiece.Type,
            AttackerTeam = triggeringPiece.Team,
            AttackerPos = triggeringPiece.CurrentPosition,

            // === 防守方 (敵人尚未生成) ===
            DefenderId = null,
            DefenderTeam = triggeringPiece.Team == Team.White ? Team.Black : Team.White,

            // 關鍵：將敵人陣容 ID 傳遞到下一個場景！
            TargetItemId = new List<string> { _targetBattleId }
        };

        Debug.Log($"[TriggerBattleEffect] 準備進入事件戰鬥！玩家 {triggeringPiece.Type} 遭遇敵人陣容: {_targetBattleId}");

        // 3. 發送全局切換場景信號！
        // 因為這裡是在 ResolutionPhase 中執行的，所以此時 UI 早已關閉。
        context.SignalBus.Fire(new RequestEnterBattleSignal
        {
            BattleType = _battleType,
            PendingBattle = eventBattleData
        });
    }
}
#endregion

public class ApplyStatusEffect : IEventEffect
{
    public string EffectId => "apply_status";

    // 未來如果狀態變多，這裡可以改成傳入 Enum 來決定 New 哪一種 Status
    private readonly int _duration;

    public ApplyStatusEffect(int duration)
    {
        _duration = duration;
    }

    public bool CanExecute(IEffectContext context)
    {
        // 必須要有觸發事件的目標棋子才能執行
        return context.TargetPieceId != null;
    }

    public void Execute(IEffectContext context)
    {
        // 1. 透過 Context 裡的 BoardQuery 找到該棋子
        var targetPiece = context.BoardQuery.GetPieceById(context.TargetPieceId); 

        if (targetPiece != null)
        {
            // 2. 實體化狀態並塞進棋子的背包裡
            var stunStatus = new StunStatus(_duration);
            targetPiece.AddStatus(stunStatus);
        }
        else
        {
            Debug.LogWarning("[ApplyStatusEffect] 找不到目標棋子，無法附加狀態！");
        }
    }
}

public class ApplyCombatModifierEffect : IEventEffect
{
    public string EffectId => "apply_combat_modifier";
    private readonly CombatModifierType _type;
    private readonly int _duration;

    public ApplyCombatModifierEffect(CombatModifierType type, int duration)
    {
        _type = type;
        _duration = duration;
    }

    public bool CanExecute(IEffectContext context) => true;

    public void Execute(IEffectContext context)
    {
        context.CombatModifierService.AddModifier(_type, _duration);
    }
}