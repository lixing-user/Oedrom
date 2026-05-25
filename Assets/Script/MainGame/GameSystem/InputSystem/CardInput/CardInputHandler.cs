using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public interface ICardInputManager
{
    bool IsCardInputEnabled { get; }
    void SetCardInputEnabled(bool enabled);

    InputData CollectInputData();
    bool HasActiveCardInput { get; }
    void ForceReset();

    void OnCardPointerDown(CardID cardId, PointerEventData eventData);
    void OnCardDrag(CardID cardId, PointerEventData eventData);
    void OnCardPointerUp(CardID cardId, PointerEventData eventData);
}

public class CardInputHandler : ICardInputManager
{
    [Inject] private ICardInputService _inputService;
    [Inject] private ICoordinateConverter _converter;
    private Camera _mainCamera;

    private InputData _lastInputData;
    private bool _isProcessingCard;

    public bool HasActiveCardInput => _isProcessingCard;

    private readonly LayerMask _boardLayerMask;
    private readonly float _defaultYHeight = 0.25f;

    public bool IsCardInputEnabled { get; private set; } = true;

    public CardInputHandler()
    {
        _mainCamera = Camera.main;

        // 用於卡牌放置時檢測棋盤位置（排除預覽物件）
        _boardLayerMask = LayerMask.GetMask("Tiles", "Terrain", "Board");
    }


    #region 監測輸入事件
    public void SetCardInputEnabled(bool enabled)
    {
        IsCardInputEnabled = enabled;
        Debug.Log($"[CardInputManager] 卡牌輸入: {(enabled ? "開啟" : "關閉")}");
    }

    public void OnCardPointerDown(CardID cardId, PointerEventData eventData)
    {
        Debug.Log("[CardInput] pick");
        _isProcessingCard = true;

        InputData inputData = CreateInputData(cardId, eventData, true, false, false);
        _lastInputData = inputData;
        _inputService.ProcessInput(inputData);
    }

    public void OnCardDrag(CardID cardId, PointerEventData eventData)
    {
        Debug.Log("[CardInput] drag");

        InputData inputData = CreateInputData(cardId, eventData, false, true, false);
        _lastInputData = inputData;
        _inputService.ProcessInput(inputData);
    }

    public void OnCardPointerUp(CardID cardId, PointerEventData eventData)
    {
        Debug.Log("[CardInput] release");
        _isProcessingCard = false;

        InputData inputData = CreateInputData(cardId, eventData, false, false, true);
        _inputService.ProcessInput(inputData);
    }
    #endregion

    #region 核心邏輯
    /// <summary>
    /// 集中的 InputData 生成邏輯（包含完整的棋盤位置檢測）
    /// </summary>
    private InputData CreateInputData(
        CardID cardId,
        PointerEventData eventData,
        bool isDown,
        bool isDragging,
        bool isUp)
    {
        Ray ray = _mainCamera.ScreenPointToRay(eventData.position);

        bool hitBoard = Physics.Raycast(
         ray,
         out RaycastHit hit,
         Mathf.Infinity,
         _boardLayerMask
        );

        Vector3 worldPosition;
        TilePosition tilePosition = null;

        if (hitBoard)
        {
            worldPosition = hit.point;
            tilePosition = _converter.WorldToGridIndex(hit.point);

            Debug.Log($"[CardInputManager] 擊中棋盤: {hit.collider.name}, Tile: {tilePosition}");
        }
        else
        {
            // 備用方案：與平面相交
            worldPosition = GetFallbackWorldPosition(ray, eventData.position);

            Debug.Log($"[CardInputManager] 未擊中棋盤，使用備用座標: {worldPosition}");
        }

        return new InputData
        {
            Source = InputSource.CardUI,
            IsMouseDown = isDown,
            IsDragging = isDragging,
            IsMouseUp = isUp,

            // 棋盤位置資訊
            HasHit = hitBoard,
            Hit = hit,
            IsOverBoard = hitBoard,
            HitTilePosition = tilePosition,
            MouseWorldPosition = worldPosition,

            ScreenPosition = eventData.position,
            EventData = eventData,
            Timestamp = Time.time,
            HitCard = cardId,
            IsOverUI = true  // 來自 CardUI，必然在 UI 上
        };
    }

    public InputData CollectInputData()
    {
        // 檢查 _lastInputData 是否已初始化（預設值 Source 為 Board3D）
        if (_lastInputData.Source == InputSource.Board3D)
        {
            return new InputData { Source = InputSource.CardUI };
        }
        return _lastInputData;
    }

    /// <summary>
    /// 備用方案：當沒有擊中棋盤時，與平面相交計算世界座標
    /// </summary>
    private Vector3 GetFallbackWorldPosition(Ray ray, Vector2 screenPos)
    {
        Plane boardPlane = new Plane(Vector3.up, new Vector3(0, _defaultYHeight, 0));

        if (boardPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // 最終備用：使用螢幕座標轉換
        Vector3 fallbackPos = _mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 10f)
        );
        fallbackPos.y = _defaultYHeight;
        return fallbackPos;
    }

    public void ForceReset()
    {
        _isProcessingCard = false;

        // 實例化一個乾淨的 InputData 覆蓋舊緩存，確保 IsDragging 被重置為 false
        _lastInputData = new InputData
        {
            Source = InputSource.CardUI,
            IsDragging = false,
            IsMouseDown = false
        };

        _inputService.ForceCancel();
    }
    #endregion
}
