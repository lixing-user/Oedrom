using ChessPieceCardProduction;
using DG.Tweening;
using UnityEngine;
using Zenject;


public class PieceView : MonoBehaviour
{
    #region 屬性
    [Header("棋子基本信息")]
    [SerializeField] private string _pieceIdValue;
    [SerializeField] private string _sourceCardIdValue;

    [Header("棋盤位置")]
    [SerializeField] private int _currentX;
    [SerializeField] private int _currentY;
    [SerializeField] private Team _currentTeam;

    [Inject] private ChessPiece _model;
    [Inject] private BoardSettings _settings;
    [Inject(Id = "InitialPosition")] private Vector3 _initialPos;

    protected MeshRenderer[] _renderers;
    protected MaterialPropertyBlock _block;
    private Material _teamMaterial;

    #endregion

    #region 初始化
    private void Awake()
    {
        InitializeRenderer();
        //gameObject.layer = LayerMask.NameToLayer("Pieces");
        if (_model != null)
        {
            _pieceIdValue = _model.Id.ToString();

            _sourceCardIdValue = _model.SourceCardId != null
             ? _model.SourceCardId.ToString()
             : "None (非卡牌來源)";

            var pos = _model.CurrentPosition;
            if (pos != null)
            {
                _currentX = pos.X;
                _currentY = pos.Y;
                _currentTeam = _model.Team;
            }
        }
    }

    private void InitializeRenderer()
    {
        if (_renderers == null)
        {
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            _block = new MaterialPropertyBlock();

            if (_renderers == null || _renderers.Length == 0)
            {
                Debug.LogWarning($"[PieceView] {_pieceIdValue} 找不到 MeshRenderer，材質變色將失效！");
            }
        }
    }
    #endregion

    #region 公開方法（供 Spawner 調用）
    public void SetPosition(Vector3 worldPos)
    {
        UpdateWorldPosition(worldPos);
    }

    public void SetYRotation(float yAngle)
    {
        transform.rotation = Quaternion.Euler(0, yAngle, 0);
    }

    public Vector3 GetTargetScale()
    {
        //transform.localScale = scale;
        float scale = _settings != null ? _settings.PieceVisualSize : 1.0f;
        return Vector3.one * scale;
    }

    public void SetMaterial(Material material)
    {
        _teamMaterial = material;
        ApplyMaterial(material);
    }

    /// 暫時換成其他材質（例如待命區），不覆蓋隊伍材質
    public void SetTemporaryMaterial(Material material) => ApplyMaterial(material);

    /// 還原成隊伍材質（從待命區回到棋盤時呼叫）
    public void RestoreTeamMaterial() => ApplyMaterial(_teamMaterial);

    private void ApplyMaterial(Material material)
    {
        if (material == null) return;
        InitializeRenderer();

        foreach (var r in _renderers)
        {
            // 取得整個材質陣列，每個槽位都換掉
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = material;
            r.sharedMaterials = mats;

            // 清掉舊的 PropertyBlock，避免覆蓋新材質的屬性
            r.SetPropertyBlock(null);
        }
    }

    public void UpdateTeamVisual(Team newTeam)
    {
        if (_renderers == null) return;

        // 1. 改變材質顏色 (這裡假設你用 MaterialPropertyBlock 控制顏色)
        if (_block == null) _block = new MaterialPropertyBlock();

        // 根據新隊伍設定顏色，這部分依你的 Shader 屬性名稱為準 (例如 "_Color" 或 "_BaseColor")
        Color teamColor = (newTeam == Team.White) ? Color.white : Color.black;
        _block.SetColor("_BaseColor", teamColor);
        foreach (var r in _renderers)
        {
            r.SetPropertyBlock(_block);
        }

        // 2. 改變面向 (同你 Registry 的邏輯，讓它馬上轉向)
        float targetYRotation = (newTeam == Team.White) ? 0f : 180f;

        // 你可以選擇直接轉向：
        transform.rotation = Quaternion.Euler(0, targetYRotation, 0);

        // 或者是如果你有寫 DOTween 等動畫，這裡可以做個平滑旋轉：
        // transform.DORotate(new Vector3(0, targetYRotation, 0), 0.5f);
    }

    public void SetLayer(string layerName)
    {
        transform.gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void SyncLogicalPosition()
    {
        if (_model != null)
        {
            if (_model.CurrentPosition != null)
            {
                _currentX = _model.CurrentPosition.X;
                _currentY = _model.CurrentPosition.Y;
                _currentTeam = _model.Team;
            }
            else
            {
                // 標記為 -1，代表目前不在棋盤上（例如在待命區或已陣亡）
                _currentX = -1;
                _currentY = -1;
                _currentTeam = _model.Team;
            }
        }
    }
    #endregion

    #region 動畫邏輯
    private void UpdateWorldPosition(Vector3 worldPos)
    {
        // 更新位置
        transform.position = worldPos;

        var boardPos = _model.CurrentPosition;
        if (boardPos != null)
        {
            _currentX = boardPos.X;
            _currentY = boardPos.Y;
        }

        Debug.Log($"[PieceView] {_model.Id} 位置已更新到 {worldPos}, 棋盤座標 ({_currentX}, {_currentY})");
    }

    // 一般移動
    public Tween DOMoveTo(Vector3 targetWorldPos, float duration)
    {
        // 讓棋子面向目標並移動
        transform.DOLookAt(targetWorldPos, 0.2f, AxisConstraint.Y);
        return transform.DOMove(targetWorldPos, duration).SetEase(Ease.InOutSine);
    }

    // 戰鬥跳躍 (示範更豐富的 DOTween 應用)
    public Tween DOJumpTo(Vector3 targetWorldPos, float jumpPower, int numJumps, float duration)
    {
        transform.DOLookAt(targetWorldPos, 0.1f, AxisConstraint.Y);
        return transform.DOJump(targetWorldPos, jumpPower, numJumps, duration);
    }

    // 陣亡縮放消失
    public Tween DODie()
    {
        return transform.DOScale(Vector3.zero, 0.5f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(gameObject));
    }
    #endregion

    #region Domain 查詢（供其他系統使用）
    /// <summary>
    /// 獲取 Domain Model 的 ID（用於 Repository 查詢）
    /// </summary>
    public PieceID GetModelId() => _model?.Id;
    public CardID GetSourceCardId() => _model?.SourceCardId;
    public bool IsFromCard() => _model?.SourceCardId != null;

    public ChessPieceType GetPieceType() => _model?.Type ?? ChessPieceType.None;
    public Team GetTeam() => _model?.Team ?? Team.White;
    public PiecePosition GetPosition() => _model?.CurrentPosition;
    public bool IsCaptured() => _model?.IsCaptured ?? false;
    public bool HasMoved() => _model?.HasMoved ?? false;
    #endregion

    public class Factory : PlaceholderFactory<ChessPieceType, ChessPiece, Vector3, PieceView> //Vector3  參數是「初始世界座標」
    {
    }
}
