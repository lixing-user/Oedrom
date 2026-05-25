using UnityEngine;

public struct StoryInputData
{
    public bool AdvanceRequested;   // 點空白區 or Space
    public bool SkipHeld;           // Ctrl 壓住
    public bool IsPointerOverUI;    // 點在 UI 按鈕上
}
