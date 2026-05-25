using UnityEngine;

public class GuideRuntimeModel
{
    public int CurrentChapterIndex { get; set; } = 0;
    public int CurrentPageIndex { get; set; } = 0;

    public void ResetState()
    {
        CurrentChapterIndex = 0;
        CurrentPageIndex = 0;
    }
}
