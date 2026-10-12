using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPresenter : MonoBehaviour
{
    private ILobbyView view;
    private LobbyModel model;

    private bool isQuitting;

    private void Start()
    {
        view = GetComponent<ILobbyView>();
        model = new LobbyModel();

        view.OnStartClicked += HandleStartClicked;
        view.OnLibraryClicked += HandleLibraryClicked;
        view.OnQuitClicked += HandleQuitClicked;
        view.OnBackClicked += HandleBackToMainMenu;
        view.OnQuitGameClicked += HandleQuitGame;
        model.OnStateChanged += HandleStateChanged;

        model.SetState(LobbyState.MainMenu);
    }
    private void OnDestroy()
    {
        if (view != null)
        {
            view.OnStartClicked -= HandleStartClicked;
            view.OnLibraryClicked -= HandleLibraryClicked;
            view.OnQuitClicked -= HandleQuitClicked;
            view.OnBackClicked -= HandleBackToMainMenu;
            view.OnQuitGameClicked -= HandleQuitGame;
        }
        if (model != null)
            model.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStartClicked() => model.SetState(LobbyState.StartGame);
    private void HandleLibraryClicked() => model.SetState(LobbyState.Library);
    private void HandleQuitClicked() => model.SetState(LobbyState.Quit);
    private void HandleBackToMainMenu() => model.SetState(LobbyState.MainMenu);

    private void HandleQuitGame()
    {
        if (isQuitting) return;          // 防止連點重複觸發
        isQuitting = true;
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        // view.ShowClosingState();      // 可選：在 ILobbyView 加一個「關閉中」畫面並停用按鈕
        yield return null;               // 讓 UI 先渲染一兩幀
        yield return null;

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
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