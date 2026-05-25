using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/FocusLandmark")]
public class FocusLandmarkAction : MicroActionSO
{
    [Header("目標地標的網格位置")]
    public Vector2Int TargetPosition;

    public override void Execute(SceneGameActionContext context)
    {
        var landmarkView = context.LandmarkViewRegistry.GetViewAt(TargetPosition);

        if (landmarkView != null)
        {
            // 完全複用現有的 Signal，直接把地標的 Transform 丟給攝影機！
            context.Global.SignalBus.Fire(new CameraFocusRequestSignal(landmarkView.transform));
            Debug.Log($"[FocusLandmarkAction] 攝影機聚焦於地標: {TargetPosition}");
        }
        else
        {
            Debug.LogWarning($"[FocusLandmarkAction] 位置 {TargetPosition} 上找不到地標 View！");
        }
    }
}