using UnityEngine;

public class TutorialDialogueLine
{
    public Sprite avatarSprite;
    public string characterName;
    public string content;
    public DialogueBoxAnchor anchor;

    public TutorialDialogueLine(Sprite avatar, string name, string content, DialogueBoxAnchor anchor)
    {
        this.avatarSprite = avatar;
        this.characterName = name;
        this.content = content;
        this.anchor = anchor;
    }
}
