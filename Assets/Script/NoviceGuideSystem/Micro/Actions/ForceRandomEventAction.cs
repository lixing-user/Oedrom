using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/Force Random Event")]
public class ForceRandomEventActionSO : MicroActionSO
{
    [Header("事件控制選項")]
    [Tooltip("如果勾選，該次事件判定將強制為『無事發生』，壓制原本的隨機機制。")]
    public bool SuppressEvent = false;

    [Tooltip("想要強制觸發的事件（如果勾選了 SuppressEvent，此欄位將被忽略）")]
    public RandomEventData SpecificEvent;

    public override void Execute(SceneGameActionContext context)
    {
        // 透過 Context 取得你的全局覆寫服務
        var overrideService = context.Global.TutorialOverrideService;

        if (overrideService == null)
        {
            Debug.LogError("[ForceRandomEventActionSO] 找不到 ITutorialOverrideService！");
            return;
        }

        if (SuppressEvent)
        {
            // 塞入 null 代表強制不觸發事件
            //overrideService.EnqueueForcedEvent(null);
            //Debug.Log("[Tutorial] 安插指令：強制【壓制】下一次隨機事件");
            overrideService.SetRandomEventSuppressed(true);
            Debug.Log("[Tutorial] 已鎖定：教學期間持續壓制隨機事件");
            Debug.Log($"[TutorialAction] 持有的鎖 ID: {overrideService.GetHashCode()}");
        }
        else if (SpecificEvent != null)
        {
            // 塞入特定事件
            overrideService.EnqueueForcedEvent(SpecificEvent);
            Debug.Log($"[Tutorial] 安插指令：強制觸發事件 {SpecificEvent.name}");
        }
        else
        {
            Debug.LogWarning("[ForceRandomEventActionSO] 設定無效：既沒有壓制事件，也沒有指定事件。");
        }
    }
}