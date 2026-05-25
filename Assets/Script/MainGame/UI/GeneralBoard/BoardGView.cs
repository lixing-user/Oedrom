using UnityEngine;

public class BoardGView : MonoBehaviour, IBoardGView
{
    #region Field & Property
    [Header("Screens")]
    [SerializeField] private GameObject EndBattleUI;
    [SerializeField] private CanvasGroup UIcanvasGroup;

    [Header("Buttons")]
    [SerializeField] private ButtonView CloseResultButton;

    [Header("Panels")]
    [SerializeField] private PanelView VictoryScreen;
    [SerializeField] private PanelView DefeatScreen;

    public event System.Action OnCloseResultConfirmed;

    public event System.Action OnBackClicked;
    #endregion

    void Start()
    {
        HideEndBattleUI();
        HideEndScreens();

        CloseResultButton.OnClicked += () => OnCloseResultConfirmed?.Invoke();

        VictoryScreen.OnBackRequested += () => OnBackClicked?.Invoke();
        DefeatScreen.OnBackRequested += () => OnBackClicked?.Invoke();
    }
    void OnDestroy()
    {
        CloseResultButton.OnClicked -= () => OnCloseResultConfirmed?.Invoke();

        VictoryScreen.OnBackRequested -= () => OnBackClicked?.Invoke();
        DefeatScreen.OnBackRequested += () => OnBackClicked?.Invoke();
    }

    public void ShowEndBattleUI()
    {
        EndBattleUI.SetActive(true);
        UIcanvasGroup.alpha = 1;
        UIcanvasGroup.interactable = true;
        UIcanvasGroup.blocksRaycasts = true;
    }
    public void HideEndBattleUI()
    {
        EndBattleUI.SetActive(false);
        UIcanvasGroup.alpha = 0;
        UIcanvasGroup.interactable = false;
        UIcanvasGroup.blocksRaycasts = false;
    }

    public void ShowVictoryScreen()
    {
        Debug.Log("Show Victory Scene");
        VictoryScreen.Show();
    }

    public void ShowDefeatScreen()
    {
        Debug.Log("Show Defeated Scene");
        DefeatScreen.Show();
    }

    public void HideEndScreens()
    {
        VictoryScreen.Hide();
        DefeatScreen.Hide();
    }
}

public interface IBoardGView
{
    event System.Action OnCloseResultConfirmed;
    event System.Action OnBackClicked;
    void ShowEndBattleUI();
    void HideEndBattleUI();
    void ShowVictoryScreen();
    void ShowDefeatScreen();
    void HideEndScreens();
}