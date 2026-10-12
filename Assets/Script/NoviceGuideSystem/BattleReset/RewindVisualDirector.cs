using System;
using System.Collections;
using UnityEngine;
using Zenject;

public interface IRewindVisualPresenter
{
    IEnumerator PlayCoverIn();
    IEnumerator PlayCoverOut();
}

public class RewindVisualDirector : IRewindVisualPresenter
{
    [Inject] private RewindTransitionView _view;


    public IEnumerator PlayCoverIn()
    {
        Debug.Log("[RewindDirector] 播放遮罩淡入動畫 (進場)...");
        _view.PlayRewindEffect(Team.White);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator PlayCoverOut()
    {
        Debug.Log("[RewindVisualPresenter] 撤除遮蔽動畫 (退場)...");

        bool done = false;
        _view.FadeOutEffects(() => done = true);
        yield return new WaitUntil(() => done);
    }
}


public struct RewindSequenceFinishedSignal { }