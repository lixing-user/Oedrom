using TMPro;
using UnityEngine;

public class BoardFView : MonoBehaviour, IBoardFView
{
    [Header("Setup")]
    [SerializeField] private PanelView setupPanel;
    [SerializeField] private PanelView startGamePanel;
    [SerializeField] public ButtonView autoArrangeButton;
    [SerializeField] public ButtonView resetButton;
    [SerializeField] public ButtonView startGameButton;

    [Header("Resolution")]
    [SerializeField] private PanelView MGFSceneTransitionPanel;
    [SerializeField] private ButtonView endGameButton;

    [Header("Deployment Info")]
    [SerializeField] private TMP_Text deploymentCountText;

    public event System.Action OnArrangeRequest;
    public event System.Action OnResetRequest;
    public event System.Action OnStartGameClicked;
    //public event System.Action OnUndoClicked;
    public event System.Action OnBackMainGameRequest;
    public event System.Action OnBackClicked;

    #region 初始化&綁定
    void Start()
    {
        autoArrangeButton.OnClicked += () => OnArrangeRequest?.Invoke();
        resetButton.OnClicked += () => OnResetRequest?.Invoke();
        startGameButton.OnClicked += () => OnStartGameClicked?.Invoke();
        //undoButton.OnClicked += () => OnUndoClicked?.Invoke();
        endGameButton.OnClicked += () => OnBackMainGameRequest?.Invoke();

        MGFSceneTransitionPanel.OnBackRequested += () => OnBackClicked?.Invoke();
    }

    void OnDestroy()
    {
        autoArrangeButton.OnClicked -= () => OnArrangeRequest?.Invoke();
        resetButton.OnClicked -= () => OnResetRequest?.Invoke();
        startGameButton.OnClicked -= () => OnStartGameClicked?.Invoke();
        //undoButton.OnClicked -= () => OnUndoClicked?.Invoke();
        endGameButton.OnClicked -= () => OnBackMainGameRequest?.Invoke();

        MGFSceneTransitionPanel.OnBackRequested += () => OnBackClicked?.Invoke();
    }
    #endregion

    #region 互動內容
    public void ShowSetupPanel()
    {
        setupPanel.Show();
        startGamePanel.Show();
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

    public void HideAllPanels()
    {
        MGFSceneTransitionPanel.Hide();
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

public interface IBoardFView
{
    public event System.Action OnArrangeRequest;
    public event System.Action OnResetRequest;
    public event System.Action OnStartGameClicked;
    //public event System.Action OnUndoClicked;
    public event System.Action OnBackMainGameRequest;
    public event System.Action OnBackClicked;

    void ShowSetupPanel();
    void HideSetupPanel();
    void ShowMGFScenePanel();
    void HideAllPanels();
    void UpdateDeploymentInfo(int currentCount, int maxCount);
}
