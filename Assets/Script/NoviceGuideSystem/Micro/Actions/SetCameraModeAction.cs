using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/SetCameraMode")]
public class SetCameraModeAction : MicroActionSO
{
    [Header("目標攝影機模式")]
    [Tooltip("僅適用於不需要動態目標的模式 (如 Default, StandbyZone, Freelook)")]
    public CameraMode TargetMode;

    public override void Execute(SceneGameActionContext context)
    {
        // 直接發送切換模式的 Signal，不需要依賴 BoardQuery
        context.Global.SignalBus.Fire(new CameraModeRequestSignal(TargetMode));
    }
}