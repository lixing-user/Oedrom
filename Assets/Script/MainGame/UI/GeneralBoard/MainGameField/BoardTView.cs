using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardTView : MonoBehaviour, IBoardTView
{
    #region Fields and Properties
    [Header("Panels")]
    [SerializeField] public PanelView battleEventUI;
    [SerializeField] public PanelView shopEventUI;
    [SerializeField] public PanelView randomEventUI;
    [SerializeField] public PanelView backToLobbyUI;

    [Header("Buttons")]
    [SerializeField] public ButtonView guideButton;
    [SerializeField] public ButtonView backToLobbyRequestButton;
    [SerializeField] public ButtonView backToLobbyButton;
    [SerializeField] public ButtonView battleEventButton;
    [SerializeField] public ButtonView shopEventButton;

    [Header("Dynamic Elements")]
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private Transform optionsContainer;

    [Header("Screens")]
    [SerializeField] private GameObject VictoryScreen;
    [SerializeField] private GameObject DefeatScreen;

    [Header("Random Event Content")]
    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private TextMeshProUGUI eventDescriptionText;
    [SerializeField] private Image eventIconImage;

    public event System.Action OnBattleConfirmed;
    public event System.Action OnShopEntered;

    public event System.Action<int> OnRandomEventOptionSelected;
    private List<ButtonView> _spawnedOptionButtons = new List<ButtonView>();

    public event System.Action OnBackToLobbyRequest;
    public event System.Action OnBackToLobbyConfirmed;
    public event System.Action OnGuideClicked;

    public event System.Action OnBackClicked;
    #endregion

    #region 初始化&綁定
    void Start()
    {
        battleEventButton.OnClicked += () => OnBattleConfirmed?.Invoke();
        shopEventButton.OnClicked += () => OnShopEntered?.Invoke();

        backToLobbyRequestButton.OnClicked += () => OnBackToLobbyRequest?.Invoke();
        backToLobbyButton.OnClicked += () => OnBackToLobbyConfirmed?.Invoke();
        guideButton.OnClicked += () => OnGuideClicked?.Invoke();

        battleEventUI.OnBackRequested += () => OnBackClicked?.Invoke();
        shopEventUI.OnBackRequested += () => OnBackClicked?.Invoke();
        randomEventUI.OnBackRequested += () => OnBackClicked?.Invoke();
        backToLobbyUI.OnBackRequested += () => OnBackClicked?.Invoke();
    }
    void OnDestroy()
    {
        battleEventButton.OnClicked -= () => OnBattleConfirmed?.Invoke();
        shopEventButton.OnClicked -= () => OnShopEntered?.Invoke();

        backToLobbyRequestButton.OnClicked -= () => OnBackToLobbyRequest?.Invoke();
        backToLobbyButton.OnClicked -= () => OnBackToLobbyConfirmed?.Invoke();
        guideButton.OnClicked -= () => OnGuideClicked?.Invoke();

        battleEventUI.OnBackRequested -= () => OnBackClicked?.Invoke();
        shopEventUI.OnBackRequested -= () => OnBackClicked?.Invoke();
        randomEventUI.OnBackRequested -= () => OnBackClicked?.Invoke();
        backToLobbyUI.OnBackRequested -= () => OnBackClicked?.Invoke();
    }
    #endregion

    #region 互動內容
    public void ShowBattleEventPanel()
    {
        HideAllPanels();
        battleEventUI.Show();
    }
    public void ShowShopEventPanel()
    {
        HideAllPanels();
        shopEventUI.Show();
    }
    public void ShowRandomEventPanel()
    {
        HideAllPanels();
        randomEventUI.Show();
    }
    public void ShowBackToLobbyPanel()
    {
        HideAllPanels();
        backToLobbyUI.Show();
    }


    public void HideAllPanels()
    {
        backToLobbyUI.Hide();
        battleEventUI.Hide();
        shopEventUI.Hide();
        randomEventUI.Hide();
    }


    public void SetRandomEventContent(string name, string description, Sprite icon, IReadOnlyList<EventOptionDTO> options)
    {
        if (eventTitleText != null) eventTitleText.text = name;
        if (eventDescriptionText != null) eventDescriptionText.text = description;
        if (eventIconImage != null)
        {
            eventIconImage.sprite = icon;
            eventIconImage.gameObject.SetActive(icon != null);
        }

        // 2. 隱藏並清理所有已經生成的選項按鈕 (對抗事件疊加)
        foreach (var btn in _spawnedOptionButtons)
        {
            btn.gameObject.SetActive(false);
            btn.ClearListeners(); // 極度重要：清空上一次事件綁定的委派
        }

        // 3. 根據當前事件的選項數量，動態配置按鈕
        for (int i = 0; i < options.Count; i++)
        {
            var optionDTO = options[i];

            // 閉包陷阱：務必使用區域變數將 Index 鎖定在當前迴圈圈數
            int capturedIndex = optionDTO.OptionIndex;

            ButtonView btnView;

            // 判斷是要「重複使用」還是「生成新的」
            if (i < _spawnedOptionButtons.Count)
            {
                btnView = _spawnedOptionButtons[i];
            }
            else
            {
                GameObject newBtnObj = Instantiate(optionButtonPrefab, optionsContainer);
                btnView = newBtnObj.GetComponent<ButtonView>();
                _spawnedOptionButtons.Add(btnView);
            }

            // 啟用按鈕並設定內容
            btnView.gameObject.SetActive(true);
            btnView.SetContent(optionDTO.OptionName, optionDTO.Description);

            // 綁定點擊事件
            btnView.OnClicked += () => {
                // 這裡呼叫的是 BoardTView 的 event，通知 Presenter 玩家選了哪個 Index
                OnRandomEventOptionSelected?.Invoke(capturedIndex);
            };
        }
    }
    #endregion
}

public interface IBoardTView
{
    event System.Action OnBattleConfirmed;
    event System.Action OnShopEntered;
    event System.Action<int> OnRandomEventOptionSelected;

    event System.Action OnBackToLobbyRequest;
    event System.Action OnBackToLobbyConfirmed;
    event System.Action OnBackClicked;

    void ShowBattleEventPanel();
    void ShowShopEventPanel();
    void ShowRandomEventPanel();
    void SetRandomEventContent(string name, string description, Sprite icon, IReadOnlyList<EventOptionDTO> options);
    void ShowBackToLobbyPanel();
    void HideAllPanels();
}