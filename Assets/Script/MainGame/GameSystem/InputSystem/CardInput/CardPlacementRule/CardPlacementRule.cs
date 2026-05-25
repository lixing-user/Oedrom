using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ICardPlacementRule
{
    // 給視覺層：取得所有合法的放置格子，用來發光引導
    List<Vector2Int> GetValidPlacementTiles();

    // 給邏輯層：判斷某個座標是否可以放置
    bool IsValidPlacement(Vector2Int position);
}


#region MainGame Rules
public class BottomRowPlacementRule : ICardPlacementRule
{
    [InjectOptional] private readonly ITutorialOverrideService _tutorialOverride;

    private readonly BoardDimensions _dimensions;

    public BottomRowPlacementRule(BoardDimensions dimensions)
    {
        _dimensions = dimensions;
    }

    public List<Vector2Int> GetValidPlacementTiles()
    {
        var tiles = new List<Vector2Int>();
        for (int x = 0; x < _dimensions.BoardSizeX; x++)
        {
            // === UI 高亮過濾層 ===
            // 如果教學系統有限制落點，就不把其他格子加入高亮清單
            var pos = new Vector2Int(x, 0);
            if (_tutorialOverride != null && _tutorialOverride.IsPlacementRestricted(pos))
            {
                continue;
            }

            tiles.Add(new Vector2Int(x, 0));
        }
        return tiles;
    }

    public bool IsValidPlacement(Vector2Int position)
    {
        // 1. 教學攔截檢查
        if (_tutorialOverride != null && _tutorialOverride.IsPlacementRestricted(position))
        {
            Debug.LogWarning("[教學系統] 玩家試圖放置在錯誤的位置或使用了錯誤的卡牌！已攔截。");
            return false;
        }

        return position.y == 0 && position.x >= 0 && position.x < _dimensions.BoardSizeX;
    }
}
#endregion

#region Battle Session Rules
public class DynamicPhasePlacementRule : ICardPlacementRule
{
    private readonly BoardDimensions _dimensions;
    private readonly IFlowFacade _flowFacade;
    private readonly IResourceManager _resourceManager;
    private readonly IPieceDeploymentService _deploymentService;

    public DynamicPhasePlacementRule(
        BoardDimensions dimensions, 
        IFlowFacade gameModel,
        IResourceManager resourceManager,
        IPieceDeploymentService deploymentService)
    {
        _dimensions = dimensions;
        _flowFacade = gameModel;
        _resourceManager = resourceManager;
        _deploymentService = deploymentService;
    }

    private bool IsMoraleLimitReached()
    {
        float currentMorale = _resourceManager.GetVariable(VariableType.morale);
        int maxAllowedPieces = Mathf.FloorToInt(currentMorale) + 5;
        int currentPieceCount = _deploymentService.GetDeployedPieceCount();

        return currentPieceCount >= maxAllowedPieces;
    }

    public List<Vector2Int> GetValidPlacementTiles()
    {
        var tiles = new List<Vector2Int>();

        if (_flowFacade.GetCurrentFlow() == GameFlow.Setup) // 假設遊戲還沒開始 (佈陣階段)
        {
            // 底部 2 排
            if (IsMoraleLimitReached())
            {
                return tiles;
            }

            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                tiles.Add(new Vector2Int(x, 0));
                tiles.Add(new Vector2Int(x, 1));
            }
        }
        else // 遊戲進行中
        {
            // 角落四格
            int maxX = _dimensions.BoardSizeX - 1;
            int maxY = _dimensions.BoardSizeY - 1;
            tiles.Add(new Vector2Int(0, 0));
            tiles.Add(new Vector2Int(maxX, 0));
            tiles.Add(new Vector2Int(0, maxY));
            tiles.Add(new Vector2Int(maxX, maxY));
        }
        return tiles;
    }

    public bool IsValidPlacement(Vector2Int position)
    {
        if (_flowFacade.GetCurrentFlow() == GameFlow.Setup)
        {
            // 如果士氣達上限，拒絕放置操作
            if (IsMoraleLimitReached())
            {
                Debug.LogWarning("[CardPlacement] 放置失敗：已達當前士氣可放置的棋子上限。");
                return false;
            }

            return position.y >= 0 && position.y <= 1 && position.x >= 0 && position.x < _dimensions.BoardSizeX;
        }
        else
        {
            // 遊戲進行中 (維持你原本的邏輯)
            int maxX = _dimensions.BoardSizeX - 1;
            int maxY = _dimensions.BoardSizeY - 1;
            return (position.x == 0 && position.y == 0) ||
                   (position.x == maxX && position.y == 0) ||
                   (position.x == 0 && position.y == maxY) ||
                   (position.x == maxX && position.y == maxY);
        }
    }
}
#endregion