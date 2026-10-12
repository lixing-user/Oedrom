using System.Collections.Generic;
using UnityEngine;

public class StoryProgressManager
{
    private StoryProgressModel _progressModel;
    private StoryChapterConfig _currentChapter;

    private Queue<StoryChapterConfig> _storyQueue = new Queue<StoryChapterConfig>();

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

    // 讓外部 (Action) 可以把故事排進隊列中
    public void EnqueueChapter(StoryChapterConfig chapter)
    {
        if (chapter != null)
        {
            _storyQueue.Enqueue(chapter);
            Debug.Log($"[StoryQueue] 已將章節加入排隊: {chapter.chapterName}");
        }
    }

    public bool TryDequeueNextChapter(out StoryChapterConfig nextChapterToPlay)
    {
        if (_storyQueue.Count > 0)
        {
            nextChapterToPlay = _storyQueue.Dequeue();
            _currentChapter = nextChapterToPlay; // 設置為當前正在播的章節
            return true;
        }

        nextChapterToPlay = null;
        return false;
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
