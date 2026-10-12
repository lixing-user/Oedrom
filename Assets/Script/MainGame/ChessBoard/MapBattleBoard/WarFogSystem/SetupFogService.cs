using System;
using UnityEngine;
using Zenject;

public interface ISetupFogService
{
    void ApplySetupFog();
    void ClearFog();
}

public class SetupFogService : ISetupFogService, IInitializable
{
    private readonly BattleContextHolder _battleContext;
    private readonly BoardDimensions _dimensions; // 從 Domain 取得棋盤尺寸
    private readonly ICombatModifierService _modifierService; // 取得 Domain 規則
    private readonly IFogRenderer _fogRenderer;


    // 將計算屬性快取，提高可讀性
    private int Width => _dimensions.BoardSizeX;
    private int Height => _dimensions.BoardSizeY;


    #region LifeCycle
    public SetupFogService(
        BattleContextHolder battleContext,
        BoardDimensions dimensions,
        ICombatModifierService modifierService,
        IFogRenderer fogRenderer)
    {
        _battleContext = battleContext;
        _dimensions = dimensions;
        _modifierService = modifierService;
        _fogRenderer = fogRenderer;

        _fogRenderer.InitializeFogMap(Width, Height);
    }

    public void Initialize()
    {
        
    }
    #endregion

    public void ApplySetupFog()
    {
        var battle = _battleContext.CurrentBattle;

        if (battle == null || battle.DefenderTeam != Team.White)
        {
            _fogRenderer.ClearFog();
            return;
        }

        // 向 Domain 詢問佈陣極限
        int maxRow = _modifierService.GetMaxDeploymentRow();

        // 建立資料陣列，傳遞給 Renderer
        bool[] visibilityMap = GenerateVisibilityMap(maxRow);
        _fogRenderer.RenderFog(visibilityMap);
    }

    public void ClearFog()
    {
        _fogRenderer.ClearFog();
    }

    // 專注於繪圖邏輯：根據 Domain 給的限制塗色
    private bool[] GenerateVisibilityMap(int maxVisibleRow)
    {
        bool[] map = new bool[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = y * Width + x;
                map[index] = y < maxVisibleRow; // true 為可見
            }
        }
        return map;
    }
}
