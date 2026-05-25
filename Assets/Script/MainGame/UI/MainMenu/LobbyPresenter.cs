using UnityEngine;
using UnityEngine.UI;

public class LobbyPresenter : MonoBehaviour
{
    private ILobbyView view;
    private LobbyModel model;

    private void Start()
    {
        view = GetComponent<ILobbyView>();
        model = new LobbyModel();

        view.OnStartClicked += () => HandlePanelRequested(LobbyState.StartGame);
        view.OnLibraryClicked += () => HandlePanelRequested(LobbyState.Library);
        view.OnQuitClicked += () => HandlePanelRequested(LobbyState.Quit);
        view.OnBackClicked += HandleBackToMainMenu;
        view.OnQuitGameClicked += () => Application.Quit();

        model.OnStateChanged += HandleStateChanged;

        model.SetState(LobbyState.MainMenu);
    }

    private void HandlePanelRequested(LobbyState newState)
    {
        model.SetState(newState);
    }

    private void HandleBackToMainMenu()
    {
        model.SetState(LobbyState.MainMenu);
    }

    private void HandleStateChanged(LobbyState newState)
    {
        switch (newState)
        {
            case LobbyState.MainMenu:
                view.HideAllPanels();
                break;

            case LobbyState.StartGame:
                view.ShowStartPanel();
                break;

            case LobbyState.Library:
                view.ShowLibraryPanel();
                break;

            case LobbyState.Quit:
                view.ShowQuitPanel();
                break;
        }
    }
}