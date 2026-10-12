using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class BoardNView : MonoBehaviour, IBoardNView
{
    [Header("Setup")]
    [SerializeField] private PanelView setupPanel;
    [SerializeField] private PanelView startGamePanel;
    [SerializeField] public ButtonView autoArrangeButton;
    [SerializeField] public ButtonView resetButton;
    [SerializeField] public ButtonView startGameButton;

    [Header("Resolution")]
    [SerializeField] private PanelView MGFSceneTransitionPanel;
    [SerializeField] private ButtonView backMGFButton;

    [SerializeField] private PanelView CBSSceneTransitionPanel;
    [SerializeField] private ButtonView engageCBButton;

    [Header("Deployment Info")]
    [SerializeField] private TMP_Text deploymentCountText;

    public event System.Action OnArrangeRequest;
    public event System.Action OnResetRequest;
    public event System.Action OnStartGameClicked;
    public event System.Action OnBackMainGameRequest;
    public event System.Action OnChaseBattleRequest;
    public event System.Action OnBackClicked;

    #region 初始化&綁定
    void Start()
    {
        autoArrangeButton.OnClicked += () => OnArrangeRequest?.Invoke();
        resetButton.OnClicked += () => OnResetRequest?.Invoke();
        startGameButton.OnClicked += () => OnStartGameClicked?.Invoke();
        backMGFButton.OnClicked += () => OnBackMainGameRequest?.Invoke();
        engageCBButton.OnClicked += () => OnChaseBattleRequest?.Invoke();

        MGFSceneTransitionPanel.OnBackRequested += () => OnBackClicked?.Invoke();
        CBSSceneTransitionPanel.OnBackRequested += () => OnBackClicked?.Invoke();
    }

    void OnDestroy()
    {
        autoArrangeButton.OnClicked -= () => OnArrangeRequest?.Invoke();
        resetButton.OnClicked -= () => OnResetRequest?.Invoke();
        startGameButton.OnClicked -= () => OnStartGameClicked?.Invoke();
        backMGFButton.OnClicked -= () => OnBackMainGameRequest?.Invoke();
        engageCBButton.OnClicked -= () => OnChaseBattleRequest?.Invoke();


        MGFSceneTransitionPanel.OnBackRequested -= () => OnBackClicked?.Invoke();
        CBSSceneTransitionPanel.OnBackRequested -= () => OnBackClicked?.Invoke();
    }
    #endregion

    #region 互動內容
    public void ShowSetupPanel()
    {
        startGamePanel.Show();
        setupPanel.Show();
    }
    public void HideSetupPanel()
    {
        setupPanel.Hide();
        startGamePanel.Hide();
    }

    public void ShowMGFScenePanel()
    {
        HideAllPanels();
        MGFSceneTransitionPanel.Show();
    }

    public void ShowCBSScenePanel()
    {
        CBSSceneTransitionPanel.Show();
    }

    public void HideAllPanels()
    {
        MGFSceneTransitionPanel.Hide();
        CBSSceneTransitionPanel.Hide();
    }
    #endregion

    #region 佈陣資訊更新
    public void UpdateDeploymentInfo(int currentCount, int maxCount)
    {
        if (deploymentCountText != null)
        {
            // 根據需求可加入顏色變化，例如超過或等於上限時變紅
            string colorHex = currentCount >= maxCount ? "<color=red>" : "<color=white>";
            deploymentCountText.text = $"[佈陣數量] \n {colorHex}{currentCount}</color> / {maxCount}";
        }
    }
    #endregion
}

public interface IBoardNView
{
    public event System.Action OnArrangeRequest;
    public event System.Action OnResetRequest;
    public event System.Action OnStartGameClicked;
    public event System.Action OnBackMainGameRequest;
    public event System.Action OnChaseBattleRequest;
    public event System.Action OnBackClicked;

    void ShowSetupPanel();
    void HideSetupPanel();
    void ShowMGFScenePanel();
    void ShowCBSScenePanel();
    void HideAllPanels();
    void UpdateDeploymentInfo(int currentCount, int maxCount);
}
