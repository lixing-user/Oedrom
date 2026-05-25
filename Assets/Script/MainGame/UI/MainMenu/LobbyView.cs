using UnityEngine;

public class LobbyView : MonoBehaviour, ILobbyView
{
    [Header("Panels")]
    [SerializeField] private PanelView StartGamePanel;
    [SerializeField] private PanelView LibraryPanel;
    [SerializeField] private PanelView QuitPanel;

    [Header("Buttons")]
    [SerializeField] private ButtonView startButton;
    [SerializeField] private ButtonView libraryButton;
    [SerializeField] private ButtonView quitPanelButton;
    [SerializeField] private ButtonView quitGameButton;

    public event System.Action OnStartClicked;
    public event System.Action OnLibraryClicked;
    public event System.Action OnQuitClicked;
    public event System.Action OnBackClicked;
    public event System.Action OnQuitGameClicked;


    void Start()
    {
        startButton.OnClicked += () => OnStartClicked?.Invoke();
        libraryButton.OnClicked += () => OnLibraryClicked?.Invoke();
        quitPanelButton.OnClicked += () => OnQuitClicked?.Invoke();
        quitGameButton.OnClicked += () => OnQuitGameClicked?.Invoke();

        //關閉UIpanel事件訂閱
        StartGamePanel.OnBackRequested += () => OnBackClicked?.Invoke();
        LibraryPanel.OnBackRequested += () => OnBackClicked?.Invoke();
        QuitPanel.OnBackRequested += () => OnBackClicked?.Invoke();
    }
    private void OnDestroy()
    {
        startButton.OnClicked -= () => OnStartClicked?.Invoke();
        libraryButton.OnClicked -= () => OnLibraryClicked?.Invoke();
        quitPanelButton.OnClicked -= () => OnQuitClicked?.Invoke();
        quitGameButton.OnClicked -= () => OnQuitGameClicked?.Invoke();

        //關閉UIpanel事件訂閱解除
        StartGamePanel.OnBackRequested -= () => OnBackClicked?.Invoke();
        LibraryPanel.OnBackRequested -= () => OnBackClicked?.Invoke();
        QuitPanel.OnBackRequested -= () => OnBackClicked?.Invoke();
    }

    public void ShowStartPanel()
    {
        HideAllPanels();
        StartGamePanel.Show();
    }

    public void ShowLibraryPanel()
    {
        HideAllPanels();
        LibraryPanel.Show();
    }

    public void ShowQuitPanel()
    {
        HideAllPanels();
        QuitPanel.Show();
    }

    public void HideAllPanels()
    {
        StartGamePanel.Hide();
        LibraryPanel.Hide();
        QuitPanel.Hide();
    }
}

public interface ILobbyView
{
    event System.Action OnStartClicked;
    event System.Action OnLibraryClicked;
    event System.Action OnQuitClicked;
    event System.Action OnBackClicked;
    event System.Action OnQuitGameClicked;

    void ShowStartPanel();
    void ShowLibraryPanel();
    void ShowQuitPanel();
    void HideAllPanels();
}