using UnityEngine;
using UnityEngine.UI;

public class BoardEView : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PanelView setupPanel;
    [SerializeField] public ButtonView startGameButton;

    [Header("UI 元件綁定")]
    [SerializeField] private Transform _tokenContainer; // 裝 Token 的 Layout Group 容器 (右下角)

    [SerializeField] private GameObject _TimerBarPrefab; // 單一步數的圓點圖示 Prefab
    [SerializeField] private Image _TimerBarPrefabImage;

    [Header("Resolution")]
    [SerializeField] private PanelView EndGamePanel;
    [SerializeField] private ButtonView EndGameButton;

    public event System.Action OnStartGameClicked;
    public event System.Action OnEndGameRequest;

    private void Start()
    {
        startGameButton.OnClicked += () => OnStartGameClicked?.Invoke();
        EndGameButton.OnClicked += () => OnEndGameRequest?.Invoke();
    }
    private void OnDestroy()
    {
        startGameButton.OnClicked -= () => OnStartGameClicked?.Invoke();
        EndGameButton.OnClicked -= () => OnEndGameRequest?.Invoke();
    }

    #region Setup Panel
    public void ShowSetupPanel()
    {
        setupPanel.Show();
    }
    public void HideSetupPanel()
    {
        setupPanel.Hide();
    }
    #endregion

    #region Timer Countdown
    public void InitializeTimerBar()
    {
        if (_TimerBarPrefabImage != null)
        {
            _TimerBarPrefabImage.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("[BoardEView] 在生成的計時條上找不到 Image 元件！請檢查 Prefab。");
        }
    }


    public void UpdateTimerDisplay(float remainingTime, float duration)
    {
        if (_TimerBarPrefabImage == null || duration <= 0f) return;

        // 計算剩餘時間的比例 (會得到 0.0 ~ 1.0 的浮點數)
        float fillRatio = remainingTime / duration;

        // 更新 UI 的 Fill Amount 數值
        _TimerBarPrefabImage.fillAmount = fillRatio;

        // (額外視覺優化) 當時間剩下不到 20% 時，可以讓計時條變成紅色警告玩家
        if (fillRatio <= 0.2f)
        {
            _TimerBarPrefabImage.color = Color.red;
        }
        else
        {
            _TimerBarPrefabImage.color = Color.white; // 預設顏色
        }
    }

    public void PlayTimeUpEffect()
    {
        Debug.Log("[BoardEView] 時間到！播放結束特效！");
        // 這裡可以加入例如畫面震動、計時條閃爍等 Animator 觸發
    }
    #endregion

    #region Resolution
    public void ShowEndGamePanel()
    {
        EndGamePanel.Show();
    }
    public void HideEndGamePanel()
    {
        EndGamePanel.Hide();
    }
    #endregion
}
