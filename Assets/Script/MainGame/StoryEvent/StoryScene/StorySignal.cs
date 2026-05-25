using UnityEngine;


public class DialogueEndedSignal
{
    public string ChapterId;
    public bool IsLastChapter;
}

public class StoryChapterCompletedSignal
{
    public string CompletedChapterId { get; set; }  // 剛完成的章節 ID
    public string NextChapterId { get; set; }        // 下一章 ID（null 代表全部完成）
}