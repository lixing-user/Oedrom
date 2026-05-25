using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;


public enum CardState { Idle, Hover, Dragging }


//原本的ObjectCard，掛載在各個棋子prefab上
public class CardInputInspector : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Inject] private ICardInputManager _inputManager;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] public GameObject Obj_inGame;

    //[SerializeField] private float distanceFromCamera = 10f;
    //[SerializeField] private float defaultYHeight = 0.25f;

    private CardID _cachedCardID = CardID.Empty;  // 緩存 ID
    private CardFacade _cachedFacade;  // 緩存 Facade


    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        _cachedCardID = _cachedFacade?.GetModelId();
        _cachedFacade = GetComponent<CardFacade>();
    }


    #region 業務邏輯
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"============ OnPointerDown 觸發! GameObject: {gameObject.name} ============");

        if (!_inputManager.IsCardInputEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left)return;

        _cachedCardID = _cachedFacade?.GetModelId();
        if (_cachedCardID.IsEmpty)
        {
            Debug.LogWarning("CardInputHandler: CardID 為空，無法處理輸入");
            return;
        }

        _inputManager.OnCardPointerDown(_cachedCardID, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_inputManager.IsCardInputEnabled) return;

        _inputManager.OnCardDrag(_cachedCardID, eventData);
    }
   
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_inputManager.IsCardInputEnabled) return;

        _inputManager.OnCardPointerUp(_cachedCardID, eventData);
    }
    #endregion
}