using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IPieceViewRegistry
{
    void RegisterView(PieceID pieceId, PieceView view);
    void UnregisterView(PieceID pieceId);
    PieceView GetView(PieceID pieceId);
    IEnumerable<PieceID> GetAllRegisteredIds();
    Vector3 GetWorldPosition(int x, int y);
    void ResetPieceToTileCenter(PieceID pieceId);
}

public class PieceViewRegistry : IPieceViewRegistry
{
    [Inject] private ITerrainHeightSampler _heightSampler;
    [Inject] private IBoardTileGenerator _tileGenerator;

    private Dictionary<PieceID, PieceView> _viewMap = new();

    /// 棋子生成時註冊（由 Spawner 調用）
    public void RegisterView(PieceID pieceId, PieceView view)
    {
        if (_viewMap.ContainsKey(pieceId))
        {
            Debug.LogWarning($"[PieceViewRegistry] PieceID {pieceId} 已存在，覆蓋舊 View");
        }

        _viewMap[pieceId] = view;
        Debug.Log($"[PieceViewRegistry] 註冊 View: {pieceId}");
    }

    /// 棋子被移除時取消註冊
    public void UnregisterView(PieceID pieceId)
    {
        if (_viewMap.Remove(pieceId))
        {
            Debug.Log($"[PieceViewRegistry] 移除 View: {pieceId}");
        }
    }

    public PieceView GetView(PieceID pieceId)
    {
        return _viewMap.TryGetValue(pieceId, out var view) ? view : null;
    }

    #region Helper Method
    /// <summary>
    /// 提供外部查詢指定棋盤座標的真實世界座標 (包含地形高度)
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 tileCenter = _tileGenerator.GetTileCenter(x, y);
        float terrainHeight = _heightSampler.GetTerrainHeightAt(x, y);
        return new Vector3(tileCenter.x, terrainHeight, tileCenter.z);
    }

    /// <summary>
    /// 將棋子強制校正回他所在的棋盤格子中心，並重置旋轉
    /// </summary>
    public void ResetPieceToTileCenter(PieceID pieceId)
    {
        if (pieceId == null) return;

        var view = GetView(pieceId);
        if (view == null) return;

        var boardPos = view.GetPosition();
        if (boardPos == null)
        {
            Debug.Log($"[PieceViewRegistry] 棋子 {view.GetModelId()} 已無座標(可能已陣亡)，跳過 Reset。");
            return;
        }

        // 1. 還原位置 (抵銷 0.75f 的偏移)
        Vector3 worldPos = GetWorldPosition(boardPos.X, boardPos.Y);

        view.SetPosition(worldPos);

        // 2. 還原面向 (Rotation)
        // 這裡假設你的預設面向是 Quaternion.identity，
        // 如果你有分黑白陣營的預設面向，請在這裡判斷 (例如：白棋朝前，黑棋朝後)
        float targetYRotation = (view.GetTeam() == Team.White) ? 0f : 180f;

        view.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);

        Debug.Log($"[PieceViewRegistry] 已將 {view.GetTeam()} 陣營的 {view.GetModelId()} 重置。座標:({boardPos.X},{boardPos.Y}) / Rotation Y:{targetYRotation}");
    }

    public IEnumerable<PieceID> GetAllRegisteredIds()
    {
        return _viewMap.Keys.ToList();
    }
    #endregion
}
