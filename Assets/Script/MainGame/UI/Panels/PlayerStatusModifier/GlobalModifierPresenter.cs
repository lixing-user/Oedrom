using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GlobalModifierPresenter : MonoBehaviour
{
    [Inject] private readonly ICombatModifierService _modifierService;
    [Inject] private readonly SignalBus _signalBus;

    private GlobalModifierView _view;

    void Awake()
    {
        _view = GetComponent<GlobalModifierView>();
    }

    void OnEnable()
    {
        // 訂閱：當狀態增加或扣減時，都執行刷新
        _signalBus.Subscribe<CombatModifierAddedSignal>(UpdateUI);
        _signalBus.Subscribe<CombatModifierTickedSignal>(UpdateUI);

        // 回合切換時也強制刷一次確保資料同步
        _signalBus.Subscribe<GlobalTurnChangedSignal>(UpdateUI);

        UpdateUI(); // 初始刷新
    }

    void OnDisable()
    {
        _signalBus.Unsubscribe<CombatModifierAddedSignal>(UpdateUI);
        _signalBus.Unsubscribe<CombatModifierTickedSignal>(UpdateUI);
        _signalBus.Unsubscribe<GlobalTurnChangedSignal>(UpdateUI);
    }

    private void UpdateUI()
    {
        var activeModifiers = _modifierService.GetActiveModifiers();
        var uiDatas = new List<ModifierUIData>();

        foreach (var kvp in activeModifiers)
        {
            uiDatas.Add(new ModifierUIData
            {
                Name = GetModifierDisplayName(kvp.Key),
                RemainingBattles = kvp.Value,
                // 從 Config 或特定的資源庫拿到對應修改器的圖示
                //Icon = GetModifierIcon(kvp.Key),
                //DisplayColor = GetModifierColor(kvp.Key)
            });
        }

        _view.Refresh(uiDatas);
    }


    #region 擴充視覺功能
    private string GetModifierDisplayName(CombatModifierType type)
    {
        return type switch
        {
            CombatModifierType.ScorchedEarth => "焦土作戰",
            CombatModifierType.Guerrilla => "游擊戰術",
            CombatModifierType.SupplyPlunder => "補給掠奪",
            _ => "未知效果"
        };
    }

    private Sprite GetModifierIcon(CombatModifierType type)
    {
        // 這裡建議建立一個 ModifierConfigSO，裡面對應 Enum 與 Sprite
        return null;
    }

    private Color GetModifierColor(CombatModifierType type)
    {
        return type == CombatModifierType.ScorchedEarth ? Color.red : Color.green;
    }
    #endregion
}
