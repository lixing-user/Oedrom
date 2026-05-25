using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum CombatModifierType
{
    None,
    ScorchedEarth,  // 焦土戰術 (無掉落)
    Guerrilla,      // 游擊戰 (擴大佈陣)
    SupplyPlunder   // 補給掠奪 (兩倍掉落)
}

// Domain / Services / ICombatModifierService.cs
public interface ICombatModifierService
{
    Dictionary<CombatModifierType, int> GetActiveModifiers(); // 提供給 Presenter 查詢目前清單的方法

    // === 註冊與狀態管理 ===
    void AddModifier(CombatModifierType type, int battleDuration);
    void TickModifiers(BattleSessionData session); // 戰鬥結束時呼叫，扣減場次
    void ClearAll();

    // === 提供給外部查詢的攔截器 (Hooks) ===
    float GetDropMultiplier(); // 詢問當前掉落倍率 (1.0=正常, 2.0=掠奪, 0.0=焦土)
    bool IsDeployZoneExpanded(); // 詢問是否擴大佈陣
    int GetMaxDeploymentRow();
}

public class CombatModifierService : ICombatModifierService
{
    [Inject] private SignalBus _signalBus;

    // 字典紀錄：Key = 狀態類型, Value = 剩餘戰鬥場次
    private readonly Dictionary<CombatModifierType, int> _activeModifiers = new();

    // 提供給 Presenter 查詢目前清單的方法
    public Dictionary<CombatModifierType, int> GetActiveModifiers() => _activeModifiers;


    public void AddModifier(CombatModifierType type, int battleDuration)
    {
        if (_activeModifiers.ContainsKey(type))
        {
            _activeModifiers[type] += battleDuration; // 疊加場次
        }
        else
        {
            _activeModifiers[type] = battleDuration;
        }

        _signalBus.Fire<CombatModifierAddedSignal>();
        Debug.Log($"[CombatModifier] 添加 {type}，持續 {battleDuration} 場戰鬥");
    }

    public void TickModifiers(BattleSessionData session)
    {
        // 可以根據 session.BattleType 決定要不要扣次數
        if (session.BattleType != BattleType.Normal && session.BattleType != BattleType.Final) return;

        var keys = new List<CombatModifierType>(_activeModifiers.Keys);
        foreach (var key in keys)
        {
            _activeModifiers[key]--;
            if (_activeModifiers[key] <= 0)
            {
                _activeModifiers.Remove(key);
                Debug.Log($"[CombatModifier] {key} 效果已結束");
            }
        }

        _signalBus.Fire<CombatModifierTickedSignal>();
    }


    // === 攔截邏輯 ===
    public float GetDropMultiplier()
    {
        if (_activeModifiers.ContainsKey(CombatModifierType.ScorchedEarth)) return 0f;
        if (_activeModifiers.ContainsKey(CombatModifierType.SupplyPlunder)) return 2f;
        return 1f; // 預設 1 倍
    }

    public bool IsDeployZoneExpanded()
    {
        return _activeModifiers.ContainsKey(CombatModifierType.Guerrilla);
    }

    public int GetMaxDeploymentRow()
    {
        return IsDeployZoneExpanded() ? 4 : 2; // 擴大佈陣後多一行
    }

    public void ClearAll() => _activeModifiers.Clear();
}

#region Signals
// 當新的修改器被加入時觸發
public struct CombatModifierAddedSignal { }

// 當修改器次數扣減或移除時觸發
public struct CombatModifierTickedSignal { }
#endregion