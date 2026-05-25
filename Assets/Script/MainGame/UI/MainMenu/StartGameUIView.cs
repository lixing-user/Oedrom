using UnityEngine;

public class StartGameUIView : MonoBehaviour, ISGUIView
{
    [Header("Buttons")]
    [SerializeField] private ButtonView startGameButton;
    [SerializeField] private ButtonView loadGameButton;

    public event System.Action OnStartGameClicked;
    public event System.Action OnLoadGameClicked;

    void Start()
    {
        startGameButton.OnClicked += () => OnStartGameClicked?.Invoke();
        loadGameButton.OnClicked += () => OnLoadGameClicked?.Invoke();
        BindDebugButton();
    }

    void OnDestroy()
    {
        startGameButton.OnClicked -= () => OnStartGameClicked?.Invoke();
        loadGameButton.OnClicked -= () => OnLoadGameClicked?.Invoke();
        UnbindDebugButton();
    }

    #region Debug¡]¥¼¨Ó§R°£
    [SerializeField] private ButtonView DebugRunButton;

    public event System.Action OnDebugRunClicked;

    private void BindDebugButton()
    {
        DebugRunButton.OnClicked += () => OnDebugRunClicked?.Invoke();
    }
    private void UnbindDebugButton()
    {
        DebugRunButton.OnClicked -= () => OnDebugRunClicked?.Invoke();
    }
    #endregion
}

public interface ISGUIView
{
    event System.Action OnStartGameClicked;
    event System.Action OnLoadGameClicked;

    event System.Action OnDebugRunClicked;
}