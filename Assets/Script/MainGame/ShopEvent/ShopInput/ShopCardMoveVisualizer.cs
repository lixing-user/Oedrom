using UnityEngine;
using Zenject;

public class ShopCardMoveVisualizer : MonoBehaviour, ICardVisualizerService
{
    [Inject] private DiContainer _container;

    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float dragPlaneHeight = 1.0f;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _worldDragInstance;
    private Vector3 _targetPosition;
    private bool _isUpdating;


    void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (_isUpdating && _worldDragInstance != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            UpdateDragPosition(mouseWorldPos);

            _worldDragInstance.transform.position = Vector3.Lerp(
                _worldDragInstance.transform.position,
                _targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }


    private Vector3 GetMouseWorldPosition()
    {
        // 商店裡直接用一個水平假想面來接滑鼠座標
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, dragPlaneHeight, 0));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
    }


    #region ICardVisualizerService 實作
    public void CreateDragInstance(
        ChessPieceType type, 
        GameObject prefab, 
        Vector3 worldPosition, 
        Team team)
    {
        DestroyDragInstance();

        _worldDragInstance = _container.InstantiatePrefab(
            prefab,
            worldPosition,
            Quaternion.identity, 
            null);

        // 商店的預覽物件只需要關閉碰撞，不一定要改材質
        var colliders = _worldDragInstance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders) col.enabled = false;

        _worldDragInstance.layer = LayerMask.NameToLayer("Ignore Raycast");

        _targetPosition = worldPosition;
        _isUpdating = true;
    }

    public void UpdateDragPosition(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }

    public void DestroyDragInstance()
    {
        if (_worldDragInstance != null)
        {
            Destroy(_worldDragInstance);
            _worldDragInstance = null;
        }
        _isUpdating = false;
    }

    public void ShowBrokenCardWarning()
    {
        Debug.LogWarning("[Shop] 卡牌破損，無法觸發戰鬥或販賣！");
    }

    public void PlayPlacementSuccessEffect()
    {
        Debug.Log("[Shop] 成功放開拖曳 (例如：觸發戰鬥/賣出)");
    }

    public void PlayPlacementFailedEffect()
    {
        Debug.LogWarning("[Shop] 拖曳無效 (未放置在有效區域)");
    }
    #endregion
}
