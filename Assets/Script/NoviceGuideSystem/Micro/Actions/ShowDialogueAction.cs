using NaughtyAttributes;
using System;
using UnityEngine;

// 單行對話的完整資料結構
[Serializable]
public class DialogueLineData
{
    [Header("發話者設定")]
    [Expandable] // NaughtyAttributes: 讓企劃不切換視窗就能直接修改角色資料
    public CharacterProfileSO speaker;

    // 取代原本的 overrideAvatar，改用直覺的表情選擇
    public CharacterEmotion emotion = CharacterEmotion.Normal;


    [Header("對話內容")]
    [TextArea(2, 6)]
    public string content;            // 對話文字

    [Header("顯示設定")]
    public DialogueBoxAnchor anchor = DialogueBoxAnchor.BottomLeft; // 可針對每句話設定位置

    [Tooltip("這句話播完後，是否要暫停等待外部事件 (WaitingForAction)？")]
    public bool pauseAfterThisLine = false;
}

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/ShowDialogue")]
public class ShowDialogueAction : MicroActionSO
{
    [Header("對話劇本 (支援多角色切換)")]
    public DialogueLineData[] dialogueLines;


    public override void Execute(SceneGameActionContext context)
    {
        if(context.DialogueBoxManager == null)
        {
            Debug.LogError("[ShowDialogueAction] 找不到 DialogueBoxManager，無法播放對話。");
            return;
        }
        // 只負責「調用」服務，不處理邏輯
        context.DialogueBoxManager.Load(this);
    }
}

public enum DialogueBoxAnchor { BottomLeft, BottomRight, TopLeft, TopRight }
