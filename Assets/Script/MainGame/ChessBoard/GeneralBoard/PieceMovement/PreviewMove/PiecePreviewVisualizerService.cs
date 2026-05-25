using ChessPieceCardProduction;
using DG.Tweening;
using UnityEngine;
using Zenject;

public interface IPiecePreviewVisualizerService
{
    void ShowFreeDragPreview(PieceID pieceId, Vector3 mouseWorldPos);
    void ShowSnappedPreview(PieceID pieceId, PiecePosition from, PiecePosition to);
    void ClearDraggingPreview();
    void ClearSnappedPreview();
    void ClearAllPreviews();
}

public class PiecePreviewVisualizerService : MonoBehaviour, IPiecePreviewVisualizerService
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardQueryService _board;
    [Inject] private readonly ICardConfigProvider _cardConfigProvider;
    [Inject] private readonly ICoordinateConverter _coordinateConverter;
    [Inject] private readonly ITerrainHeightSampler _heightSampler;
    [Inject] private readonly BoardSettings _settings;
    [InjectOptional] private readonly IStandbyZone _standbyZone;

    [Header("視覺設定")]
    [SerializeField] private float _smoothSpeed = 20f;
    [SerializeField] private float _hoverHeight = 0.5f; // 自由拖曳時的懸浮高度
    [SerializeField] private float _previewAlpha = 0.6f;

    private GameObject _freeDragInstance;
    private GameObject _snappedInstance;
    private Vector3 _targetDragPosition;
    private bool _isFreeDragging;

    #region Unity Lifecycle
    private void Start()
    {
        _signalBus.Subscribe<PhaseEndRequestedSignal>(OnPhaseEnd);
    }

    private void OnDestroy()
    {
        _signalBus.Unsubscribe<PhaseEndRequestedSignal>(OnPhaseEnd);
    }

    private void Update()
    {
        // 參考 CardMoveVisualizer：使用 Update 進行平滑跟隨 (Lerp)，避免滑鼠移動時殘影抖動
        if (_isFreeDragging && _freeDragInstance != null)
        {
            _freeDragInstance.transform.position = Vector3.Lerp(
                _freeDragInstance.transform.position,
                _targetDragPosition,
                Time.deltaTime * _smoothSpeed
            );
        }
    }
    #endregion

    #region Core Methods
    public void ShowFreeDragPreview(PieceID pieceId, Vector3 mouseWorldPos)
    {
        DestroyInstance(ref _snappedInstance);

        if (_freeDragInstance == null)
        {
            _freeDragInstance = CreatePreviewInstance(pieceId);
        }

        if (_freeDragInstance != null)
        {
            _isFreeDragging = true;
            float terrainY = _heightSampler.GetTerrainHeightAt(mouseWorldPos);
            _targetDragPosition = new Vector3(mouseWorldPos.x, terrainY + _hoverHeight, mouseWorldPos.z);
        }
    }


    public void ShowSnappedPreview(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        _isFreeDragging = false;
        DestroyInstance(ref _freeDragInstance);

        if (_snappedInstance == null)
        {
            _snappedInstance = CreatePreviewInstance(pieceId);
        }

        if (_snappedInstance != null)
        {
            Vector3 targetWorldPos = _coordinateConverter.PieceToWorldPosition(to);
            float terrainY = _heightSampler.GetTerrainHeightAt(to.X, to.Y);

            // 吸附狀態不懸浮，直接貼地
            _snappedInstance.transform.position = new Vector3(targetWorldPos.x, terrainY, targetWorldPos.z);
        }
    }


    public void ClearDraggingPreview()
    {
        _isFreeDragging = false;
        DestroyInstance(ref _freeDragInstance);
    }
    public void ClearSnappedPreview()
    {
        DestroyInstance(ref _snappedInstance);
    }
    public void ClearAllPreviews()
    {
        _isFreeDragging = false;
        DestroyInstance(ref _freeDragInstance);
        DestroyInstance(ref _snappedInstance);
    }
    #endregion

    #region 查詢機制
    private ChessPiece FindPiece(PieceID pieceId)
    {
        if (pieceId == null) return null;

        // 1. 查詢棋盤
        var piece = _board.GetPieceById(pieceId);
        if (piece != null) return piece;

        // 2. 查詢待命區
        if (_standbyZone != null)
        {
            return _standbyZone.GetPieceById(pieceId);
        }

        return null;
    }
    #endregion

    private void OnPhaseEnd(PhaseEndRequestedSignal signal)
    {
        ClearAllPreviews();
    }

    private GameObject CreatePreviewInstance(PieceID pieceId)
    {
        //var piece = _board.GetPieceById(pieceId);
        var piece = FindPiece(pieceId);
        if (piece == null || !_cardConfigProvider.TryGetConfig(piece.Type, out CardConfig config))
            return null;

        GameObject instance = Instantiate(config.previewPrefab);
        SetupPreviewVisuals(instance, piece.Team);
        return instance;
    }

    private void SetupPreviewVisuals(GameObject instance, Team team)
    {
        // 1. 設定 Ignore Raycast Layer 防止擋住滑鼠射線
        SetLayerRecursive(instance, LayerMask.NameToLayer("Ignore Raycast"));

        // 2. 關閉碰撞
        var colliders = instance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders) col.enabled = false;

        // 3. 處理材質與 DOTween 動畫
        var renderers = instance.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            if (_settings != null && _settings.TeamMaterials != null)
            {
                renderer.material = _settings.TeamMaterials[(int)team];
            }

            foreach (var material in renderer.materials)
            {
                material.SetFloat("_Mode", 3); // Transparent
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;

                // 初始透明度設為 0，然後用 DOTween 平滑淡入
                Color color = material.color;
                color.a = 0f;
                material.color = color;

                material.DOFade(_previewAlpha, 0.2f).SetLink(instance);
            }
        }
    }

    private void DestroyInstance(ref GameObject instance)
    {
        if (instance != null)
        {
            // SetLink 會自動處理 DOTween 的銷毀，這裡直接 Destroy 即可
            Destroy(instance);
            instance = null;
        }
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }
}
