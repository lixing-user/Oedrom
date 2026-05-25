using UnityEngine;
using Zenject;

public class StartGameUIPresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    private ISGUIView view;

    void Start()
    {
        view = GetComponent<ISGUIView>();

        view.OnStartGameClicked += () => HandleStartGame();
        view.OnLoadGameClicked += () => HandleLoadGame();

        BindDebugEvent();
    }

    private void HandleStartGame()
    {
        _signalBus.Fire(new StartGameCommandSignal { Mode = GameStartMode.NewGame });
    }

    private void HandleLoadGame()
    {
        _signalBus.Fire(new StartGameCommandSignal { Mode = GameStartMode.LoadGame });
    }

    #region Debug¡]¥¼¨Ó§R°£
    private void BindDebugEvent()
    {
        view.OnDebugRunClicked += () => HandleDebugRun();
    }

    private void HandleDebugRun()
    {
        _signalBus.Fire(new StartGameCommandSignal { Mode = GameStartMode.DebugStart });

        _signalBus.Fire(new RequestSessionStateChangeSignal
        {
            TargetState = GameSessionState.MainGame
        });
    }
    #endregion
}
