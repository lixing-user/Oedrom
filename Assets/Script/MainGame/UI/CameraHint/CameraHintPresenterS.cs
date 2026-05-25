using UnityEngine;

public class CameraHintPresenterS : CameraHintPresenter
{
    protected override void UpdateCameraHint(CameraMode mode)
    {
        string hintText = "";

        switch (mode)
        {
            case CameraMode.Default:
                hintText = "[預設視角]\nSpace/WASD: 自由 | 右鍵/F: 聚焦";
                break;

            case CameraMode.Freelook:
                hintText = "[自由視角]\nEsc: 返回預設視角 | Q/E: 旋轉 | 滾輪: 縮放";
                break;

            case CameraMode.Focus:
                hintText = "[聚焦視角]\nEsc: 返回預設視角";
                break;

            case CameraMode.Opening:
            case CameraMode.Uninitialized:
            default:
                hintText = ""; // 開場運鏡或未初始化時不顯示提示
                break;
        }

        _view.ShowCameraHint(hintText);
    }
}
