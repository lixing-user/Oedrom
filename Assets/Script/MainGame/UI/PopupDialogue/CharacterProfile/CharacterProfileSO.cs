using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Data/CharacterProfile")]
public class CharacterProfileSO : ScriptableObject
{
    [Header("基本資料")]
    public string characterName;

    [Tooltip("劇本中可能出現的其他名稱，用來共用同一套立繪")]
    public string[] aliases;

    [Header("角色立繪")]
    [ShowAssetPreview]
    public Sprite storySprite;

    [Header("角色頭像")]
    [ShowAssetPreview]
    public Sprite defaultAvatar;

    [Header("表情差分")]
    public EmotionAvatar[] avatarVariants;

    // 提供給系統獲取頭像的 API
    public Sprite GetAvatar(CharacterEmotion emotion)
    {
        if (emotion == CharacterEmotion.Normal) return defaultAvatar;

        foreach (var variant in avatarVariants)
        {
            if (variant.emotion == emotion) return variant.avatar;
        }

        // 若找不到對應表情，退回預設頭像
        return defaultAvatar;
    }

    #region API：提供給 StorySystem 獲取半身立繪
    public Sprite GetStorySprite()
    {
        return storySprite; // 退回預設大立繪
    }
    #endregion
}


#region 角色表情定義
public enum CharacterEmotion
{
    Normal,
    Happy,
    Angry,
    Sad,
    Shocked,
    Thinking
}

[Serializable]
public struct EmotionAvatar
{
    public CharacterEmotion emotion;
    [ShowAssetPreview] // NaughtyAttributes: 直接在 Inspector 顯示縮圖
    public Sprite avatar;
}
#endregion