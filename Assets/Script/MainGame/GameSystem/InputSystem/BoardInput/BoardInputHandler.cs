using System;
using UnityEngine;
using Zenject;

public enum InputState
{
    Idle,
    Pick,
    Dragging,
    PieceSelected,
}

// Interface層：接收玩家輸入（第一步）
public class BoardInputHandler : IBoardInputHandler, ITickable
{
    private readonly IBoardInputService _service;
    private readonly ICoordinateConverter _coordinateConverter;
    //private readonly BoardSettings _settings;

    private readonly LayerMask _raycastLayers;
    private readonly Camera _camera;

    public bool HasActiveBoardInput => _service.HasActivePieceOperation();

    public bool IsBoardInputEnabled { get; private set; } = true;

    public BoardInputHandler(IBoardInputService service, ICoordinateConverter converter)
    {
        _service = service;
        _coordinateConverter = converter;
        _raycastLayers = LayerMask.GetMask("Tiles", "Hover", "Highlight", "Standby", "Pieces");
        _camera = Camera.main;
    }

    public void SetBoardInputEnabled(bool enabled)
    {
        IsBoardInputEnabled = enabled;
        Debug.Log($"[BoardInputHandler] 棋盤輸入: {(enabled ? "開啟" : "關閉")}");
    }

    public void ForceReset()
    {
        // 如果有緩存的 InputData 可以在此重置
        _service.ForceCancel();
    }

    public void Tick()
    {
        if (!IsBoardInputEnabled) return;

        DrawDebugRay();
        InputData inputData = CollectInputData();
        _service.ProcessInput(inputData);
    }

    public InputData CollectInputData()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, _raycastLayers);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        bool hasMoved = Mathf.Abs(mouseX) > 0.1f || Mathf.Abs(mouseY) > 0.1f;
        bool isDragging = Input.GetMouseButton(0) && hasMoved;

        PieceView hitPieceView = null;
        RaycastHit? pieceHit = null;

        foreach (var hit in hits)
        {
            var piece = hit.collider.GetComponent<PieceView>();
            if (piece != null)
            {
                hitPieceView = piece;
                pieceHit = hit;
                break;  // 找到第一個 piece 就停止
            }
        }

        RaycastHit finalHit = pieceHit ?? (hits.Length > 0 ? hits[0] : default);
        bool hasHit = hits.Length > 0;

        int hitLayerId = hasHit ? finalHit.collider.gameObject.layer : -1;
        string hitLayerName = hitLayerId >= 0 ? LayerMask.LayerToName(hitLayerId) : "";
        bool isHitOnBoard = hitLayerName != "Standby";

        if (hasHit)
        {
            //Debug.Log($"Hit Point: { finalHit.point}, Hit Layer: {LayerMask.LayerToName(finalHit.collider.gameObject.layer)}");
        }
        if (hitPieceView != null) {
            //Debug.Log($"Hit Piece ID: {hitPieceView.GetModelId()}");
        }

        return new InputData
        {
            Source = InputSource.Board3D,
            IsMouseDown = Input.GetMouseButtonDown(0),
            IsMouseUp = Input.GetMouseButtonUp(0),
            IsRightMouseDown = Input.GetMouseButtonDown(1),
            IsDragging = isDragging,
            Hit =  finalHit,
            HasHit = hasHit,
            MouseWorldPosition = hasHit ? finalHit.point : Vector3.zero,
            HitPiece = hitPieceView?.GetModelId(),           // PieceID
            HitTilePosition = (hasHit && isHitOnBoard) ? _coordinateConverter.WorldToGridIndex(finalHit.point) : null,
            HitLayer = hitLayerId,

            IsSpacePressed = Input.GetKeyDown(KeyCode.Space),
            IsEscapePressed = Input.GetKeyDown(KeyCode.Escape),
        };
    }


    #region 手動測試（完成後刪除）
    private void DrawDebugRay()
    {
        if (!Application.isPlaying) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 繪製射線（黃色）
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow);

        // 如果擊中物體，繪製紅色球體位置
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _raycastLayers))
        {
            // Debug 沒有 DrawSphere，改用三軸交叉線表示擊中點
            Debug.DrawRay(hit.point, Vector3.up * 0.15f, Color.red);
            Debug.DrawRay(hit.point, Vector3.down * 0.15f, Color.red);
            Debug.DrawRay(hit.point, Vector3.left * 0.15f, Color.red);
            Debug.DrawRay(hit.point, Vector3.right * 0.15f, Color.red);
            Debug.DrawRay(hit.point, Vector3.forward * 0.15f, Color.red);
            Debug.DrawRay(hit.point, Vector3.back * 0.15f, Color.red);
        }
    }
    #endregion
}

public interface IBoardInputHandler
{
    InputData CollectInputData();
    bool IsBoardInputEnabled { get; }
    bool HasActiveBoardInput { get; }
    void SetBoardInputEnabled(bool enabled);
    void ForceReset();
}