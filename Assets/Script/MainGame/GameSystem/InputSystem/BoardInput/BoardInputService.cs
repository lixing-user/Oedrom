using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IBoardInputService
{
    void ProcessInput(InputData inputData);
    void ForceCancel();

    InputState GetCurrentState();
    bool HasActivePieceOperation(); // 是否有棋子操作進行中

    event Action<PieceID> OnPieceSelected;
    event Action OnSelectionCanceled;
    event Action<PieceID> OnPieceInspected;

    PieceID GetSelectedPiece();
}


// Application 層：根據當前狀態機狀態處理輸入(第二步）
public class BoardInputService : IBoardInputService
{
    [Inject] private readonly IGameBoard _repository;
    [InjectOptional] private readonly IStandbyZone _standbyZone;

    [Inject] private readonly ICoordinateConverter _coordinateConverter;
    [Inject] private readonly IBoardVisualizer _tileVisualizer;
    [Inject] private readonly IPiecePreviewVisualizerService _pieceVisualizer;

    [Inject] private readonly IMoveHandler _moveHandler;
    [Inject] private readonly IMoveModeService _moveModeService;

    [Inject] private readonly List<IPieceSelectionExtension> _selectionExtensions;

    // --- 操作層面 ---
    public event Action<PieceID> OnPieceSelected;
    public event Action OnSelectionCanceled;
    private PieceID _selectedPieceId;
    private PiecePosition _selectedPieceOrigin;

    // 紀錄當前正在處理這顆棋子的擴展
    private IPieceSelectionExtension _activeExtension;
    private PieceSelectionContext _selectionContext;

    private List<Vector2Int> _visualPositions;
    private InputState _currentState = InputState.Idle;
    private Vector2Int? _lastHoverPosition;

    // --- 資訊層面 ---
    public event Action<PieceID> OnPieceInspected;
    public event Action OnInspectionCanceled;
    private PieceID _inspectedPieceId;

    private bool _isPickMode = true;  
    private float _pickTime;          
    private Vector2 _pickMousePos;    
    private const float DRAG_THRESHOLD = 10f;
    private const float PICK_THRESHOLD = 0.2f;


    #region 安全棋子查詢機制
    /// <summary>
    /// 統一由本方法進行棋子模型查詢，涵蓋棋盤與待命區
    /// </summary>
    private ChessPiece FindPiece(PieceID pieceId)
    {
        if (pieceId == null) return null;

        // 1. 先嘗試從棋盤（IGameBoard）獲取
        var piece = _repository.GetPieceById(pieceId);
        if (piece != null) return piece;

        // 2. 若棋盤找不到，且待命區存在，則嘗試從待命區獲取
        if (_standbyZone != null)
        {
            return _standbyZone.GetPieceById(pieceId);
        }

        return null;
    }
    #endregion

    #region Input StateMachine
    public void ProcessInput(InputData inputData)
    {
        UpdateHoverVisualization(inputData);
        UpdateDragState(inputData);

        switch (_currentState)
        {
            case InputState.Idle:
                HandleIdleState (inputData);
                break;

            case InputState.Pick:
                HandlePickState(inputData);
                break;

            case InputState.PieceSelected:
                HandlePieceSelectedState(inputData);
                break;

            case InputState.Dragging:
                HandleDraggingState(inputData);
                break;
        }
    }
    private void ResetInput()
    {
        CancelSelection();
        ClearInspection();
    }

    private void CancelSelection()
    {
        if (_currentState != InputState.Idle)
        {
            _selectedPieceId = null;
            _currentState = InputState.Idle;
            OnSelectionCanceled?.Invoke();

            _selectedPieceOrigin = null;
            _selectionContext = default;
            _lastHoverPosition = null;
        }
        _tileVisualizer.RemoveHighlightTiles();
        _pieceVisualizer.ClearAllPreviews();
    }

    private void ClearInspection()
    {
        if (_inspectedPieceId != null)
        {
            _inspectedPieceId = null;
            OnInspectionCanceled?.Invoke(); // UI 監聽這個事件來執行 _view.Hide()

            if (_currentState == InputState.Idle)
            {
                _tileVisualizer.RemoveHighlightTiles();
            }
        }
    }

    public void ForceCancel()
    {
        if (_currentState != InputState.Idle)
        {
            Debug.Log("[BoardInputService] 接收到強制中斷，清理棋盤狀態");
            ResetInput(); // 內部已經包含清除選取、消除預覽與高光
            OnSelectionCanceled?.Invoke(); // 確保廣播取消事件
        }
    }
    #endregion


    #region State Methods
    private void HandleIdleState(InputData inputData)
    {
        if (!inputData.IsMouseDown)    
            return;

        if (!inputData.HasHit || inputData.HitPiece == null)
        {
            ResetInput();
            return;
        }

        if (_inspectedPieceId != inputData.HitPiece)
        {
            ResetInput();
        }


        // --- 第一階段：查看 (Inspect) ---
        //var hitPiece = _repository.GetPieceById(inputData.HitPiece);
        var hitPiece = FindPiece(inputData.HitPiece);
        _inspectedPieceId = inputData.HitPiece;
        OnPieceInspected?.Invoke(inputData.HitPiece);
        bool isStandby = inputData.HitLayer == LayerMask.NameToLayer("Standby");

        // --- 第二階段：權限檢查 ---
        // 檢查陣營
        if (hitPiece.Team == Team.Black)
        {
            Debug.Log($"[Input] 點擊黑方棋子 {hitPiece.Type}，僅顯示資訊，不予選取。");

            if (!isStandby)
            {
                // 強制使用 Board Extension 來獲取敵方的移動範圍，純展示用
                var boardExtension = _selectionExtensions.OfType<BoardPieceSelectionExtension>().FirstOrDefault();
                if (boardExtension != null)
                {
                    var context = new PieceSelectionContext { Source = PieceSelectionSource.Board };
                    var enemyMoves = boardExtension.GetValidPlacementTiles(inputData.HitPiece, context);
                    _tileVisualizer.HighlightTiles(enemyMoves);
                }         
            }
            return;
        }
        // 檢查鎖定
        if (hitPiece != null && hitPiece.IsLocked)
        {
            Debug.Log($"[Input] 棋子已鎖定，無法選取: {hitPiece.Type}");
            return; 
        }
        Debug.Log($"[Idle] HitPiece={inputData.HitPiece?.GetHashCode()}, " +
            $"found={hitPiece != null}, " +
            $"IsLocked={hitPiece?.IsLocked}");


        // --- 第三階段：選取 (Select) ---
        var sortedExtensions = GetSortedExtensions(inputData.HitLayer);

        foreach (var extension in sortedExtensions)
        {
            if (!extension.IsEnabled)
            {
                Debug.Log($"[Input] 跳過停用的擴展: {extension.ExtensionName}");
                continue;
            }

            if (extension.CanSelectPiece(inputData.HitPiece, out var context))
            {
                _selectedPieceId = inputData.HitPiece;
                OnPieceSelected?.Invoke(_selectedPieceId);  // 觸發選中棋子事件，把 ID 傳遞出去
                _selectionContext = context;  // 記錄來源（Board/Standby）
                _currentState = InputState.Pick;

                if (context.Source == PieceSelectionSource.Board)
                {
                    ChessPiece piece = _repository.GetPieceById(inputData.HitPiece);
                    if (piece != null)
                    {
                        _selectedPieceOrigin = piece.CurrentPosition;
                        Debug.Log($"[Input] 設置原始位置: {_selectedPieceOrigin}");
                    }
                    else
                    {
                        Debug.LogError($"[Input] 找不到棋子: {inputData.HitPiece}");
                        return;
                    }
                }
                else
                {
                    _selectedPieceOrigin = null;
                    Debug.Log($"[Input] 從待命區選擇，無原始位置");
                }


                _visualPositions = extension.GetValidPlacementTiles(inputData.HitPiece, context)
                ?? new List<Vector2Int>();
                _tileVisualizer.HighlightTiles(_visualPositions);

                Debug.Log($"[Input] 選擇棋子: {_selectedPieceId}, 來源: {context.Source}, 擴展: {extension.ExtensionName}");
                return;
            }
        }
        Debug.LogWarning($"[Input] 沒有啟用的擴展可以處理棋子: {inputData.HitPiece}");
    }


    private void HandlePickState(InputData inputData)
    {
        if (inputData.IsRightMouseDown)
        {
            Debug.Log("[Pick] 右鍵取消選擇");
            ResetInput();
            return;
        }

        if (!_isPickMode)
        {
            _currentState = InputState.Dragging;
            _tileVisualizer.HighlightTiles(_visualPositions);
            Debug.Log("[Picking → Dragging] 觸發拖拽閾值，進入 Drag 模式");
            return;
        }

        if (inputData.IsMouseUp)
        {
            if (inputData.HitPiece != null && !inputData.HitPiece.Equals(_selectedPieceId))
            {
                ChessPiece newPiece = _repository.GetPieceById(inputData.HitPiece);
                ChessPiece currentPiece = _repository.GetPieceById(_selectedPieceId);

                // 同隊棋子：切換選擇
                if (newPiece != null && currentPiece != null &&
                    newPiece.Team == currentPiece.Team)
                {
                    Debug.Log($"[Pick] 切換選擇到新棋子：{newPiece.Id}");
                    ResetInput();
                    ProcessInput(inputData);  // 重新選擇新棋子
                    return;
                }
            }

            // 前面已經排除了 !_isPickMode 的情況
            _currentState = InputState.PieceSelected;
            _tileVisualizer.HighlightTiles(_visualPositions);
            Debug.Log("[Picking → PieceSelected] Pick 模式");
        }
    }


    private void HandlePieceSelectedState(InputData inputData)
    {
        // 情況 A：開始拖曳（按住滑鼠移動）
        if (inputData.IsDragging)
        {
            _currentState = InputState.Dragging;
            _tileVisualizer.HighlightTiles(_visualPositions);
            //_visualizer.StartDragVisual(_selectedPieceId, inputData.MouseWorldPosition);
            Debug.Log("[Selected → Dragging]");
            return;
        }

        // 情況 B：點擊放置（快速點擊）
        if (inputData.IsMouseDown)
        {
            // 如果點擊了另一個棋子，且是同隊伍的，則直接切換選取
            if (inputData.HasHit && inputData.HitPiece != null && !inputData.HitPiece.Equals(_selectedPieceId))
            {
                ChessPiece newPiece = _repository.GetPieceById(inputData.HitPiece);
                ChessPiece currentPiece = _repository.GetPieceById(_selectedPieceId);

                if (newPiece != null && currentPiece != null && newPiece.Team == currentPiece.Team)
                {
                    Debug.Log($"[Selected] 切換選擇到新棋子：{newPiece.Id}");
                    ResetInput();            // 先關閉舊的
                    ProcessInput(inputData); // 遞迴處理新的點擊（會重新開啟面板）
                    return;
                }
            }

            PlaceChessPiece(inputData);
            return;
        }

        // 情況 C：右鍵取消
        if (inputData.IsRightMouseDown)
        {
            ResetInput();
        }

        if (inputData.HasHit && inputData.HitTilePosition != null && _selectedPieceId != null)
        {
            Vector2Int hoverVec = new Vector2Int(inputData.HitTilePosition.X, inputData.HitTilePosition.Y);

            // 只有游標在「合法移動範圍」內才顯示預覽
            if (_visualPositions.Contains(hoverVec))
            {
                PiecePosition targetPos = ConvertToPosition(inputData.HitTilePosition);
                // 若從待命區來，_selectedPieceOrigin 為 null，這裡給個保護
                PiecePosition fromPos = _selectedPieceOrigin ?? targetPos;

                _pieceVisualizer.ShowSnappedPreview(_selectedPieceId, fromPos, targetPos);
            }
            else
            {
                // 移出合法範圍時隱藏吸附預覽
                _pieceVisualizer.ClearSnappedPreview();
            }
        }
    }

    private void HandleDraggingState(InputData inputData)
    {
        // 持續更新拖曳視覺
        if (inputData.IsDragging)
        {
            if (_selectedPieceId != null)
            {
                _pieceVisualizer.ShowFreeDragPreview(_selectedPieceId, inputData.MouseWorldPosition);
            }
            return;
        }

        // 放開滑鼠 → 嘗試放置
        if (inputData.IsMouseUp)
        {
            PlaceChessPiece(inputData);
        }
    }


    private void PlaceChessPiece(InputData inputData)
    {
        if (!inputData.HasHit || inputData.HitTilePosition == null)
        {
            Debug.LogWarning("未點擊到有效格子");
            ResetInput();
            return;
        }

        PiecePosition targetPos = ConvertToPosition(inputData.HitTilePosition);

        switch (_selectionContext.Source)
        {
            case PieceSelectionSource.Board:
                PlaceFromBoard(targetPos);
                break;

            case PieceSelectionSource.Standby:
                PlaceFromStandby(targetPos);  
                break;
        }
    }

    private void PlaceFromBoard(PiecePosition targetPos)
    {
        var strategy = _moveModeService.GetCurrentStrategy();
        if (strategy == null)
        {
            Debug.LogError($"當前模式 {_moveModeService.CurrentMode} 無可用策略");
            ResetInput();
            return;
        }

        PieceID movingPiece = _selectedPieceId;
        PiecePosition originPos = _selectedPieceOrigin;

        ResetInput();

        strategy.ExecuteMove(movingPiece, originPos, targetPos);
    }

    private void PlaceFromStandby(PiecePosition targetPos)
    {
        if (!_visualPositions.Any(p => p.x == targetPos.X && p.y == targetPos.Y))
        {
            Debug.LogWarning($"[Input] 目標位置 {targetPos} 對於待命區棋子是無效的");
            ResetInput();
            return;
        }

        PieceID movingPiece = _selectedPieceId;

        ResetInput();

        bool success = _moveHandler.RestoreFromStandby(movingPiece, targetPos);

        if (success)
        {
            Debug.Log($"[Input] 從待命區放置成功: {movingPiece} → {targetPos}");
        }
        else
        {
            Debug.LogError($"[Input] 從待命區放置失敗");
        }
    }
    #endregion


    #region Helper Methods
    private PiecePosition ConvertToPosition(TilePosition tilePos)
    {
        return _coordinateConverter.GridIndexToPiecePosition(tilePos);
    }

    private void UpdateHoverVisualization(InputData inputData)
    {
        if (inputData.HitTilePosition == null)
        {
            if (_lastHoverPosition.HasValue)
            {
                _lastHoverPosition = null;
            }
            return;
        }

        Vector2Int currentPosition = new Vector2Int(
            inputData.HitTilePosition.X,
            inputData.HitTilePosition.Y
        );

        // 只在位置改變時才更新
        if (_lastHoverPosition.HasValue && _lastHoverPosition.Value == currentPosition)
        {
            return;  // 位置沒變，跳過更新
        }

        _tileVisualizer.UpdateHoverTiles(currentPosition);
        _lastHoverPosition = currentPosition;
    }

    private void UpdateDragState(InputData inputData)
    {
        if (inputData.HitTilePosition != null)
        {
            // 將 TilePosition 轉換為 Vector2Int
            var hitVector2Int = new Vector2Int(inputData.HitTilePosition.X, inputData.HitTilePosition.Y);
            _tileVisualizer.UpdateHoverTiles(hitVector2Int);
            //Debug.Log($"Hover Tile Position: {inputData.HitTilePosition}");
        }

        if (inputData.IsMouseDown)
        {
            _pickTime = Time.time;
            _pickMousePos = Input.mousePosition;
            _isPickMode = true;
        }
        else if (_currentState == InputState.Pick)
        {
            float deltaTime = Time.time - _pickTime;
            float deltaDist = Vector2.Distance(Input.mousePosition, _pickMousePos);

            if (deltaDist > DRAG_THRESHOLD || deltaTime > PICK_THRESHOLD)
            {
                _isPickMode = false;  // 進入 drag 模式
            }
        }
    }
    #endregion

    #region 查詢方法
    public InputState GetCurrentState() => _currentState;

    public bool HasActivePieceOperation()
    {
        return _currentState != InputState.Idle;
    }

    public PieceID GetSelectedPiece()
    {
        return _selectedPieceId;
    }

    private List<IPieceSelectionExtension> GetSortedExtensions(int hitLayer)
    {
        string layerName = LayerMask.LayerToName(hitLayer);

        // 根據 Layer 決定優先順序
        if (layerName == "Standby")
        {
            // 待命區 Layer：StandbyExtension 優先
            return _selectionExtensions
                .OrderByDescending(e => e.ExtensionName == "StandbyPieceSelection")
                .ToList();
        }
        else if (layerName == "Pieces")
        {
            // 棋子 Layer：BoardExtension 優先
            return _selectionExtensions
                .OrderByDescending(e => e.ExtensionName == "BoardPieceSelection")
                .ToList();
        }

        return _selectionExtensions;
    }
    #endregion
}
