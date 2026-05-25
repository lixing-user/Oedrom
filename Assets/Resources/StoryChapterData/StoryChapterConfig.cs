using System;
using System.Collections.Generic;
using UnityEngine;

public enum AfterStoryAction
{
    ContinueToNextChapter, // 繼續下一章
    SwitchToMainGame,      // 回到主地圖/新手教學
    SwitchToBattle,        // 進入戰鬥
    GoToEnding             // 進入結局
}

[CreateAssetMenu(fileName = "StoryChapterConfig", menuName = "Story/ChapterConfig")]
public class StoryChapterConfig : ScriptableObject
{
    public string chapterId;
    public string chapterName;


    [Header("對話資料")]
    public TextAsset dialogueFile; // CSV 檔案


    [Header("視覺資源")]
    public string initialBackground;
    public List<BackgroundSprite> backgroundSprite;
    public List<CharacterProfileSO> characterProfiles;
    public List<CGSprite> cgSprites;


    [Header("章節流程")]
    public StoryPhaseType[] phaseSequence; // 定義該章節的階段順序


    [Header("章節結束後")]
    public StoryChapterConfig nextChapter;
}


[Serializable]
public class BackgroundSprite
{
    public string backgroundName;
    public Sprite sprite;
}

[Serializable]
public class CGSprite
{
    public string cgName;
    public Sprite sprite;
}