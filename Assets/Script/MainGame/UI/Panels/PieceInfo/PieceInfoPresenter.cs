using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class PieceInfoPresenter : MonoBehaviour
{
    [Inject] private readonly ICardConfigProvider _configProvider;
    [Inject] private readonly IBoardInputService _inputService;
    [Inject] private readonly IBoardQueryService _boardQuery;
    [Inject] private readonly ITerrainRepository _terrainRepository;

    private PieceInfoView _view;
    private ChessPiece _currentPiece;


    #region Unity 生命週期
    void Awake()
    {
        // Awake 時就抓取 View，不需要等 SceneLoaded
        _view = GetComponent<PieceInfoView>();
        if (_view == null)
        {
            Debug.LogWarning("PieceInfoView not found on this GameObject.");
        }
    }

    void OnEnable()
    {
        if (_view != null)
        {
            BindView();
            _view.Hide(); // 初始狀態隱藏
        }
    }

    void OnDisable()
    {
        UnbindView();
    }
    #endregion

    #region Bind & Unbind View
    private void BindView()
    {
        UnbindView();
        _view.OnRightClickRequested += OnRightClick;
        _inputService.OnPieceInspected += ShowPieceInfo;
        _inputService.OnSelectionCanceled += HidePieceInfo;
    }
    private void UnbindView()
    {
        if (_view != null)
        {
            _view.OnRightClickRequested -= OnRightClick;
            _inputService.OnPieceInspected -= ShowPieceInfo;
            _inputService.OnSelectionCanceled -= HidePieceInfo;
        }
    }
    #endregion

    #region 業務邏輯 - 對外公開的方法
    public void ShowPieceInfo(ChessPiece piece)
    {
        if (piece == null)
        {
            _view.Hide();
            return;
        }

        _currentPiece = piece;

        // 通過 CardConfigProvider 獲取視覺配置
        if (_configProvider.TryGetConfig(piece.Type, out var config))
        {
            string pieceName = GetLocalizedName(piece.Type);

            string position = "無";
            string terrainStr = "無";
            if (piece.CurrentPosition != null)
            {
                position = $"({piece.CurrentPosition.X}, {piece.CurrentPosition.Y})";

                // 透過 TerrainRepository 查詢目前位置的地形
                TerrainType terrainType = _terrainRepository.GetTerrainAt(piece.CurrentPosition);
                terrainStr = GetLocalizedTerrain(terrainType);
            }

            string state = GetStateDescription(piece);

            _view.UpdatePieceInfo(pieceName, config.defaultIcon, position, state, terrainStr);

            Debug.Log($"[PieceInfoPresenter] 正在開啟 {piece.Type}({piece.Id}) 的面板，其背包內有 {piece.ActiveStatuses.Count} 個持續狀態。");

            var statusDataList = piece.ActiveStatuses.Select(s => new EventStatusUIData
            {
                Icon = s.Icon,
                Name = s.Name,
                Description = s.Description,
                RemainingTurns = s.RemainingTurns
            }).ToList();

            _view.UpdateStatuses(statusDataList);

            _view.Show();
        }
    }

    public void ShowPieceInfo(PieceID pieceId)
    {
        var piece = _boardQuery.GetPieceById(pieceId);
        ShowPieceInfo(piece);
    }

    private void HidePieceInfo()
    {
        _view.Hide();
        _currentPiece = null;
    }
    #endregion

    #region 私有方法 & 輔助方法
    private void OnRightClick()
    {
        _view.Hide();
        _currentPiece = null;
        Debug.Log("PieceInfoPanel closed by right-click.");
    }

    private string GetStateDescription(ChessPiece piece)
    {
        if (piece.IsCaptured) return "已被俘虜";

        switch (piece.State)
        {
            case PieceState.OnBoard:
                return piece.HasMoved ? "已移動" : "初始位置";
            case PieceState.InStandby:
                return "待命區";
            case PieceState.Removed:
                return "已移除";
            default:
                return "未知";
        }
    }

    private string GetLocalizedName(ChessPieceType type)
    {
        return type switch
        {
            ChessPieceType.Pawn => "步兵",
            ChessPieceType.Rook => "戰車",
            ChessPieceType.Knight => "騎士",
            ChessPieceType.Bishop => "祭司",
            ChessPieceType.Queen => "聯軍",
            ChessPieceType.King => "將軍",
            _ => type.ToString()
        };
    }

    private string GetLocalizedTerrain(TerrainType type)
    {
        return type switch
        {
            // 這裡請依照你實際定義的 TerrainType 進行替換
            TerrainType.Plain => "平原",
            TerrainType.Water => "水域",
            TerrainType.Mountain => "山脈",
            TerrainType.Forest => "森林",
            _ => type.ToString()
        };
    }
    #endregion
}