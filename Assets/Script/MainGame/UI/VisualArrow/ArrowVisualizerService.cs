using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IArrowVisualizerService
{
    /// <summary>
    /// 在 3D 棋盤上畫出拋物線箭頭
    /// </summary>
    void DrawBoardArrow(PiecePosition from, PiecePosition to);

    /// <summary>
    /// 清除所有顯示中的箭頭
    /// </summary>
    void ClearArrows();
}


public class ArrowVisualizerService :  IArrowVisualizerService
{
    [Inject] private readonly ICoordinateConverter _coordinateConverter;
    [Inject] private readonly IArrowVisualizerPresenter _presenter;


    #region Parabolic Arrow Methods
    // 由 Action 呼叫：在 3D 棋盤上畫出拋物線箭頭
    public void DrawBoardArrow(PiecePosition from, PiecePosition to)
    {
        // 透過你既有的轉換器，將棋盤座標轉為 World Space 座標
        Vector3 startWorldPos = _coordinateConverter.PieceToWorldPosition(from);
        Vector3 endWorldPos = _coordinateConverter.PieceToWorldPosition(to);

        // 稍微抬高起點，避免線條埋在棋子或地板裡
        startWorldPos += Vector3.up * 0.5f;

        // 呼叫 View 去處理實際的生成
        _presenter.SpawnArrow(startWorldPos, endWorldPos);
    }

    // 由 ClearTutorialVisualsAction 呼叫：清除所有箭頭
    public void ClearArrows()
    {
        _presenter.ClearAllArrows();
    }
    #endregion
}
