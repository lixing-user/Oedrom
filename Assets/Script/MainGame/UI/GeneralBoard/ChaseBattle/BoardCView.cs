using System.Collections.Generic;
using UnityEngine;

public class BoardCView : MonoBehaviour
{
    [Header("UI 元件綁定")]
    [SerializeField] private Transform _tokenContainer; // 裝 Token 的 Layout Group 容器 (右下角)
    [SerializeField] private GameObject _moveTokenPrefab; // 單一步數的圓點圖示 Prefab

    [Header("衛星環繞設定")]
    [SerializeField] private float _radius = 120f;       // 軌道半徑
    [SerializeField] private float _startAngle = 180f;   // 起始角度
    [SerializeField] private float _angleSpacing = -30f; // 每個 Token 的角度間距 (負數代表順時針排列)

    [Header("Resolution")]
    [SerializeField] private PanelView MGFSceneTransitionPanel;
    [SerializeField] private ButtonView backMGFButton;

    public event System.Action OnBackMainGameRequest;
    public event System.Action OnBackClicked;
    private List<GameObject> _moveTokens = new List<GameObject>();

    private void Start()
    {
        backMGFButton.OnClicked += () => OnBackMainGameRequest?.Invoke();
    }
    private void OnDestroy()
    {
        backMGFButton.OnClicked -= () => OnBackMainGameRequest?.Invoke();
    }

    // 初始化並生成最大數量的步數 Token
    public void InitializeTokens(int maxMoves)
    {
        // 1. 清理舊的 (如果有的話)
        foreach (var token in _moveTokens)
        {
            Destroy(token);
        }
        _moveTokens.Clear();

        GenerateTokens(maxMoves);

        Debug.Log($"[BoardCView] 已初始化 {maxMoves} 個 Token");
    }


    // 更新 UI 顯示，根據剩餘步數熄滅或隱藏 Token
    /// <param name="currentMoves">玩家已使用的步數</param>
    /// <param name="maxMoves">最大可用步數</param>
    public void UpdateMovesDisplay(int currentMoves, int maxMoves)
    {
        // 計算「剩餘步數」
        int movesLeft = maxMoves - currentMoves;

        for (int i = 0; i < _moveTokens.Count; i++)
        {
            // 邏輯：從列表最後一個開始扣除，或者根據 index 熄滅
            // 例如：剩餘 2 步，則 index 0, 1 為 True(亮起)，index 2 為 False(熄滅)
            bool isAvailable = i < movesLeft;

            // 這裡可以換成更酷的表現方式，例如播放「碎裂」動畫
            // 目前簡單地用 SetActive 處理
            _moveTokens[i].SetActive(isAvailable);
        }
        Debug.Log($"[BoardCView] 更新 UI：剩餘 {movesLeft} 步");
    }

    private void GenerateTokens(int maxMoves)
    {
        // 2. 根據最大步數生成 Token
        for (int i = 0; i < maxMoves; i++)
        {
            // 將 Token 生成在容器下
            GameObject token = Instantiate(_moveTokenPrefab, _tokenContainer);
            RectTransform rectTransform = token.GetComponent<RectTransform>();

            // 計算當前 Token 的角度
            float currentAngle = _startAngle + (i * _angleSpacing);
            // 將角度轉換為弧度給 Mathf.Sin/Cos 使用
            float radian = currentAngle * Mathf.Deg2Rad;

            // 圓形座標公式：x = r * cos(θ), y = r * sin(θ)
            float x = _radius * Mathf.Cos(radian);
            float y = _radius * Mathf.Sin(radian);

            // 設定相對位置 (以 Container 的中心點為基準)
            rectTransform.anchoredPosition = new Vector2(x, y);

            // (可選) 強制重置 Scale，避免生成時因為層級關係導致尺寸變形
            rectTransform.localScale = Vector3.one;

            _moveTokens.Add(token);
        }
    }

    public void ShowMGFScenePanel()
    {
        MGFSceneTransitionPanel.Show();
    }
    public void HideMGFScenePanel()
    {
        MGFSceneTransitionPanel.Hide();
    }
}
