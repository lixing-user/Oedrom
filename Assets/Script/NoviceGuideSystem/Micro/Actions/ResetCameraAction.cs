using UnityEngine;

[CreateAssetMenu(fileName = "ResetCameraAction", menuName = "Tutorial/Action/Micro/ResetCameraToDefault")]
public class ResetCameraAction : MicroActionSO
{
    public override void Execute(SceneGameActionContext context)
    {
        // 直接透過全域 SignalBus 發送重設訊號
        context.Global.SignalBus.Fire(new ResetCameraToDefaultSignal());
        Debug.Log("[ResetCameraAction] 已發送重設攝影機至預設模式的訊號。");
    }
}