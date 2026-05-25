using DG.Tweening;
using UnityEngine;
using Zenject;

public class CreditScenePresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    [Inject] private CreditModel _model;
    [Inject] private CreditSceneView _view;

    private bool _isSkipping = false;
    private Sequence _currentSequence;


    void Start()
    {
        _view.ResetView();
        PlayNextSection(0);
    }

    void OnDestroy()
    {
        _currentSequence?.Kill();
    }


    void Update()
    {
        if (!_isSkipping && Input.GetKeyDown(KeyCode.Escape))
        {
            _isSkipping = true;
            _currentSequence?.Kill();
            FinishCredits();
        }
    }


    private void PlayNextSection(int index)
    {
        if (_isSkipping || index >= _model.Sections.Count)
        {
            FinishCredits();
            return;
        }

        var section = _model.Sections[index];

        if (section.DisplayType == CreditDisplayType.Slide)
        {
            // 導演：View，播放第 N 個 Panel！
            _currentSequence = _view.PlaySlide(section.PanelIndex);
        }
        else if (section.DisplayType == CreditDisplayType.Scroll)
        {
            // 導演：View，把這串長長的字詞放進 Scroll 裡面捲動！
            _currentSequence = _view.PlayScroll(section.Content);
        }

        // 當這個動畫播完，呼叫自己播放下一個
        if (_currentSequence != null)
        {
            _currentSequence.OnComplete(() => PlayNextSection(index + 1));
        }
    }


    private void FinishCredits()
    {
        // 只要這行一發送，你的 SceneRouter 就會完美接手後續所有卸載與載入工作！
        _signalBus.Fire(new RequestSessionStateChangeSignal
        {
            TargetState = GameSessionState.MainMenu
        });
    }
}
