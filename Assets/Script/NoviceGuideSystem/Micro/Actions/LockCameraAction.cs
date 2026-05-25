using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/SetCameraLock")]
public class LockCameraAction : MicroActionSO
{
    public string LockId = "Tutorial_Action";
    public bool isLocked;

    public override void Execute(SceneGameActionContext context)
    {
        // 透過 SignalBus 發送鎖定/解鎖訊號
        context.Global.SignalBus.Fire(new CameraLockStateSignal(LockId, isLocked));
        Debug.Log($"[SetCameraLockAction] 設定鏡頭鎖定狀態為: {isLocked}");
    }
}