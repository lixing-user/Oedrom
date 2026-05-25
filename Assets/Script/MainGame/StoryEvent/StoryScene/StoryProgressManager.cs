using UnityEngine;

public class StoryProgressManager
{
    private StoryProgressModel _progressModel;
    private StoryChapterConfig _currentChapter;

    public StoryProgressManager(StoryProgressModel progressModel)
    {
        _progressModel = progressModel;
    }

    // 設定當前章節
    public void SetCurrentChapter(StoryChapterConfig chapter)
    {
        _currentChapter = chapter;
        Debug.Log($"[StoryProgressManager] 設定當前章節: {chapter.chapterName}");
    }

    // 獲取當前章節
    public StoryChapterConfig GetCurrentChapter()
    {
        return _currentChapter;
    }

    public bool IsChapterCompleted(string chapterId)
    {
        return _progressModel.HasCompletedChapter(chapterId);
    }

    // 獲取下一章
    public StoryChapterConfig GetNextChapter()
    {
        if (_currentChapter == null)
        {
            Debug.LogWarning("[StoryProgressManager] 沒有當前章節");
            return null;
        }

        return _currentChapter.nextChapter; 
    }

    // 進入下一章（並更新當前章節）
    public bool ProgressToNextChapter()
    {
        var nextChapter = GetNextChapter();

        if (nextChapter != null)
        {
            MarkChapterComplete(_currentChapter.chapterId);
            SetCurrentChapter(nextChapter);
            return true;
        }

        Debug.Log("[StoryProgressManager] 已到達最後一章");
        return false;
    }

    // 記錄進度
    public void MarkChapterComplete(string chapterId)
    {
        _progressModel.MarkChapterComplete(chapterId);
    }

    // 檢查是否還有下一章
    public bool HasNextChapter()
    {
        return _currentChapter != null && _currentChapter.nextChapter != null;
    }

    public void ResetProgress()
    {
        _progressModel.ResetAllProgress();
        _currentChapter = null;
        Debug.Log("[StoryProgressManager] 劇情進度已重置");
    }
}
