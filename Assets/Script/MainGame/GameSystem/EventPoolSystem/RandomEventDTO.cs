using UnityEngine;

public struct EventOptionDTO
{
    public int OptionIndex;      // 用於回傳給 Manager 的索引
    public string OptionName;    // 顯示在按鈕上的標題
    public string Description;   // 滑鼠懸浮或按鈕下方的敘述
}
