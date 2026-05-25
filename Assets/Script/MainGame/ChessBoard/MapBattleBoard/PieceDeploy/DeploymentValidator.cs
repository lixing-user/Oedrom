using UnityEngine;
using Zenject;


public interface IDeploymentValidator
{
    bool ValidateDeployment();
    int GetMaxDeploymentCount();
    int GetDeployedPieceCount();
}


// Application/Services/DeploymentValidator.cs
public class DeploymentValidator : IDeploymentValidator
{
    [Inject] private IGameBoard _board;
    [Inject] private BoardDimensions _dimensions;
    [Inject] private IResourceManager _resourceManager;
    [Inject] private BattleContextHolder _battleContext;
    [Inject] private ICombatModifierService _modifierService;

    /// <summary>
    /// 驗證佈陣是否合法
    /// </summary>
    public bool ValidateDeployment()
    {
        // 1. 必須有至少一個 King
        if (!HasKingOnBoard())
        {
            Debug.LogWarning("[DeploymentValidator] 佈陣失敗：必須有至少一個 King");
            return false;
        }

        // 2. 棋子必須在前兩排
        if (!AllPiecesInDeploymentZone())
        {
            Debug.LogWarning("[DeploymentValidator] 佈陣失敗：所有棋子必須在前兩排");
            return false;
        }

        // 3. 士氣上限驗證
        float currentMorale = _resourceManager.GetVariable(VariableType.morale);
        int maxAllowedPieces = GetMaxDeploymentCount(); // 計算士氣對應的佈陣上限
        int currentPieceCount = GetDeployedPieceCount();

        if (currentPieceCount > maxAllowedPieces)
        {
            Debug.LogWarning($"[DeploymentValidator] 佈陣失敗：當前士氣({currentMorale})最多只能放置 {maxAllowedPieces} 枚棋子，目前已放置 {currentPieceCount} 枚。");
            return false;
        }

        Debug.Log("[DeploymentValidator] 佈陣驗證通過");
        return true;
    }

    /// <summary>
    /// 檢查是否有 King
    /// </summary>
    private bool HasKingOnBoard()
    {
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var pos = new PiecePosition(x, y);
                var piece = _board.GetPieceAt(pos);

                if (piece != null &&
                    piece.Team == Team.White)// &&
                    //piece.Type == ChessPieceType.King)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 檢查所有白方棋子是否在前兩排
    /// </summary>
    private bool AllPiecesInDeploymentZone()
    {
        // 動態取得邊界，正常是 2，游擊戰是 4
        int minY = _modifierService.IsDeployZoneExpanded() ? 4 : 2;

        for (int y = minY; y < _dimensions.BoardSizeY; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var pos = new PiecePosition(x, y);
                var piece = _board.GetPieceAt(pos);

                if (piece != null && piece.Team == Team.White)
                {
                    return false;  // 發現白方棋子在前兩排之外
                }
            }
        }

        return true;
    }

    public int GetDeployedPieceCount()
    {
        int count = 0;
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var piece = _board.GetPieceAt(new PiecePosition(x, y));
                if (piece != null && piece.Team == Team.White)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public int GetMaxDeploymentCount()
    {
        float currentMorale = _resourceManager.GetVariable(VariableType.morale);
        return Mathf.FloorToInt(currentMorale) + 5;
    }
}