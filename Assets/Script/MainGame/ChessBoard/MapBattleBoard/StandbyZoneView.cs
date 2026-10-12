using DG.Tweening;
using UnityEngine;
using Zenject;

public enum LayoutMode
{
    Vertical,
    Horizontal,
    Grid
}

public class StandbyZoneView : MonoBehaviour
{
    [Header("顯示控制")]
    [SerializeField] private Transform visualRoot;

    [Header("待命區設定")]
    [SerializeField] private Transform whiteStandbyArea;  // 白方待命區位置
    [SerializeField] private Transform blackStandbyArea;  // 黑方待命區位置

    [Header("布局參數")]
    [SerializeField] private StandbyLayoutConfig layoutConfig;
    public StandbyLayoutConfig Config => layoutConfig;

    [Header("視覺效果")]
    [SerializeField] private float moveAnimationSpeed = 0.5f;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("待命區視覺")]
    [SerializeField] private Material standbyMaterial;

    [Inject] private readonly IPieceViewRegistry _viewRegistry;


    // 1. 提供錨點資訊供 Service (大腦) 查詢
    public Vector3 GetAnchorPosition(Team team)
    {
        if (team == Team.White && whiteStandbyArea != null) return whiteStandbyArea.position;
        if (team == Team.Black && blackStandbyArea != null) return blackStandbyArea.position;

        Debug.LogError($"[StandbyZoneView] 缺少 {team} 方的錨點設定！");
        return Vector3.zero;
    }

    #region Public Methods
    // 負責在待命區內平滑排列
    public void AnimatePieceToPosition(PieceID pieceId, Vector3 targetWorldPos)
    {
        var view = _viewRegistry.GetView(pieceId);
        if (view != null)
        {
            view.DOJumpTo(targetWorldPos, jumpPower: 1.5f, numJumps: 1, duration: moveAnimationSpeed);
        }
    }

    // 負責從待命區飛回棋盤
    public void AnimatePieceToBoard(PieceID pieceId, Vector3 targetWorldPos, System.Action onComplete = null)
    {
        var view = _viewRegistry.GetView(pieceId);
        if (view != null)
        {
            view.DOJumpTo(targetWorldPos, jumpPower: 2.0f, numJumps: 1, duration: 0.6f)
            .OnComplete(() =>
            {
                // 動畫結束：恢復為棋子 Layer，使其能再次被點擊/選取
                view.gameObject.layer = LayerMask.NameToLayer("Pieces");
                view.RestoreTeamMaterial();
                // 執行外部傳入的後續動作 (如資料同步)
                onComplete?.Invoke();
            });
        }
    }

    // 負責憑空生成的特效演出
    public void PlaySpawnEffect(PieceID pieceId, Vector3 targetWorldPos)
    {
        var view = _viewRegistry.GetView(pieceId);
        if (view != null)
        {
            view.transform.position = targetWorldPos;
            view.transform.localScale = Vector3.zero;
            view.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void UpdateVisibility(bool isLocked)
    {
        // 解鎖就放大回原比例 (Vector3.one)，上鎖就縮小到看不見 (Vector3.zero)
        Vector3 targetScale = isLocked ? Vector3.zero : Vector3.one;

        // 加上 SetEase(Ease.OutBack) 會有 Q 彈的效果
        visualRoot.DOScale(targetScale, animationDuration).SetEase(Ease.OutBack);
    }

    public void SetPieceToStandbyMaterial(PieceID pieceId)
    {
        var view = _viewRegistry.GetView(pieceId);
        if (view != null && standbyMaterial != null)
        {
            view.SetTemporaryMaterial(standbyMaterial);
        }
    }
    #endregion
}
