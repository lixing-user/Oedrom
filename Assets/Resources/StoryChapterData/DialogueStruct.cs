using UnityEngine;

// 對話行的資料結構
public class DialogueLine
{
    public int id;
    public string type; // "#" 普通對話, "&" 選項, "END" 結束
    public string characterName;
    public string position; // "left" or "right"
    public CharacterSpritePosition charPos;
    public string content;
    public int nextId;
    public string effectTag; // 特效/差分標籤

    public bool IsNormalDialogue => type == "#";
    public bool IsEnd => type == "END";
    public bool IsCG => type == "CG";
    public bool IsBackgroundChange => type == "BG";
    public bool IsSoundEffect => type == "SE";
    public bool IsAnimation => type == "ANIM";
    public bool IsCameraEffect => type == "CAMERA";
}

public class DialogueOption
{
    public int id;
    public string text;
    public int nextDialogueId;
}

public enum CharacterSpritePosition
{
    Left,
    Right,
    Center,
    None // 用於角色離場或無站位
}

public enum DialogueEffect
{
    None,   // 無特效
    Think,  // 思考模式（唯獨說話者亮）
    Mask,    // 全遮罩模式（所有人與背景都變暗）
}