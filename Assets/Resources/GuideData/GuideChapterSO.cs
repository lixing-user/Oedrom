using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideChapter", menuName = "Guide/Chapter")]
public class GuideChapterSO : ScriptableObject
{
    public string chapterId;
    public string chapterName; // 對應標籤頁按鈕文字
    public Sprite chapterIcon; // 標籤頁圖示

    [Header("分頁內容")]
    public List<GuidePageData> pages;
}

[Serializable]
public class GuidePageData
{
    public string title;
    [TextArea(3, 10)]
    public string description;
    public Sprite illustration; // 規則說明的插圖
    // 如果需要更複雜的佈局，甚至可以在這裡加入「走法演示」的 Prefab 引用
}