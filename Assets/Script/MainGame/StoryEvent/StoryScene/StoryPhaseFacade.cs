using UnityEngine;
using Zenject;

public class StoryPhaseFacade : MonoBehaviour 
{
    [Inject] private StoryStateMachine _stateMachine;
    [Inject] private StoryProgressManager _progressManager;

    [SerializeField] private StoryChapterConfig _defaultChapter;

    private void Start()
    {
        if (_progressManager.TryDequeueNextChapter(out var nextChapter))
        {
            _stateMachine.StartChapter(nextChapter);
        }
        else
        {
            var chapter = _progressManager.GetCurrentChapter() ?? _defaultChapter;
            _stateMachine.StartChapter(chapter);
        }
    }
}
