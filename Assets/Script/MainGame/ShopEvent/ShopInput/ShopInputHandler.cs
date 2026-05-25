using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class ShopInputHandler : ICardInputManager, ITickable
{
    private readonly SignalBus _signalBus;
    private readonly ICardInputService _service;

    private readonly LayerMask _shopLayers;
    private Camera _mainCamera;

    public bool IsCardInputEnabled { get; private set; } = true;
    private bool _isProcessingCard = false; 
    public bool HasActiveCardInput => _isProcessingCard;
    private InputData _lastInputData;


    public ShopInputHandler(ICardInputService service, SignalBus signalBus)
    {
        _service = service;
        _signalBus = signalBus;
        _mainCamera = Camera.main;
        _shopLayers = LayerMask.GetMask("ShopItem", "BattleTriggerArea", "SellTriggerArea", "RepairTriggerArea");        // 監聽商店裡的 3D 棋子與戰鬥觸發區
    }

    #region 輸入偵測
    public void Tick()
    {
        if (!_isProcessingCard)
        {
            HandleMouseInput();
        }

        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 1000f, _shopLayers);

        #region Test
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 0.1f);
            if (hasHit)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            }
        #endregion

        string hitItemId = null;
        if (hasHit)
        {
            // 嘗試獲取掛在模型上的 Proxy (此時 Proxy 退化為單純的資料容器)
            var itemProxy = hit.collider.GetComponentInParent<ShopItem3DClickProxy>();
            if (itemProxy != null)
            {
                hitItemId = itemProxy.ItemId;
            }
        }

        InputData data = new InputData
        {
            Source = InputSource.Board3D,
            IsMouseDown = Input.GetMouseButtonDown(0),
            IsMouseUp = Input.GetMouseButtonUp(0),
            HasHit = hasHit,
            HitLayer = hasHit ? hit.transform.gameObject.layer : 0,
            MouseWorldPosition = hasHit ? hit.point : Vector3.zero,
            HitShopItemId = hitItemId,
            Timestamp = Time.time
        };

        _service.ProcessInput(data);
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _signalBus.Fire(new RequestSectionChangeSignal { Direction = -1 });
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _signalBus.Fire(new RequestSectionChangeSignal { Direction = 1 });
        }
    }
    #endregion

    #region ICardInputManager 實作 (處理 UI 卡牌拖曳)
    public void SetCardInputEnabled(bool enabled)
    {
        IsCardInputEnabled = enabled;
    }

    public InputData CollectInputData()
    {
        return _lastInputData;
    }

    public void OnCardPointerDown(CardID cardId, PointerEventData eventData)
    {
        if (!IsCardInputEnabled) return;

        _isProcessingCard = true; // 鎖定狀態：告訴 Tick 不要再送一般射線了

        InputData data = CreateCardInputData(cardId, eventData, isDown: true, isDragging: false, isUp: false);
        _lastInputData = data;
        _service.ProcessInput(data);

        Debug.Log("[CardInput] pick");
    }

    public void OnCardDrag(CardID cardId, PointerEventData eventData)
    {
        if (!IsCardInputEnabled || !_isProcessingCard) return;

        InputData data = CreateCardInputData(cardId, eventData, isDown: false, isDragging: true, isUp: false);
        _lastInputData = data;
        _service.ProcessInput(data);

        Debug.Log("[CardInput] drag");
    }

    public void OnCardPointerUp(CardID cardId, PointerEventData eventData)
    {
        if (!IsCardInputEnabled) return;

        InputData data = CreateCardInputData(cardId, eventData, isDown: false, isDragging: false, isUp: true);
        _lastInputData = data;
        _service.ProcessInput(data);

        Debug.Log("[CardInput] release");
        _isProcessingCard = false; // 解除鎖定：恢復 Tick 的一般 3D 偵測
    }


    private InputData CreateCardInputData(
        CardID cardId,
        PointerEventData eventData,
        bool isDown,
        bool isDragging,
        bool isUp)
    {
        Ray ray = _mainCamera.ScreenPointToRay(eventData.position);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 1000f, _shopLayers);

        return new InputData
        {
            Source = InputSource.CardUI,
            HitCard = cardId,
            IsMouseDown = isDown,
            IsDragging = isDragging,
            IsMouseUp = isUp,

            ScreenPosition = eventData.position,
            EventData = eventData,
            IsOverUI = true,
            Timestamp = Time.time,

            // 將 3D 物理碰撞資訊打包進去，交給 Service 判斷
            HasHit = hasHit,
            HitLayer = hasHit ? hit.transform.gameObject.layer : 0,
            MouseWorldPosition = hasHit ? hit.point : Vector3.zero
        };
    }

    public void ForceReset()
    {
        _isProcessingCard = false;

        _lastInputData = new InputData
        {
            Source = InputSource.CardUI,
            IsDragging = false,
            IsMouseDown = false
        };

        _service.ForceCancel();
    }
    #endregion
}
