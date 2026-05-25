using System;
using UnityEngine;

//區域 (Local)。只存在於某一段教學演出的那幾十秒內。
[CreateAssetMenu(fileName = "TutorialStep", menuName = "Scriptable Objects/TutorialStep")]
public class TutorialStep : ScriptableObject
{
    public string stepId;

    [TextArea(3, 10)]
    public string objective;// 階段目標

    [Header("進入此步驟時立刻執行 (例如：跳出對話、顯示高光)")]
    public MicroActionSO[] _actionsOnStart;// 步驟開始時執行

    [Header("卡點條件 (例如：等待對話播完、等待玩家移動棋子)")]
    public MicroConditionSO[] _completionConditions;// 完成條件

    [Header("失敗條件 (例如：棋子走到錯誤的格子)")]
    public MicroConditionSO[] _failureConditions;// 失敗條件

    [Header("條件達成後執行 (例如：發放獎勵、隱藏高光)")]
    public MicroActionSO[] _actionsOnCompletion;// 完成後動作

    [Header("失敗時的動作 (例如：跳出提示對話框)")]
    public MicroActionSO[] _actionsOnFailure;// 失敗動作
}
