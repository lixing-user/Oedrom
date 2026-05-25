using System.Collections.Generic;
using System.Linq;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

public interface IRewardService
{
    IEnumerable<ChessPiece> GetAvailableRewards();
    List<RewardUIData> GetRewardUIData(); // 增加數據轉換方法
    void ClaimCardReward(PieceID pieceId);

    ResourceRewardData GenerateResourceRewards();
    void ClaimResourceRewards();
}

public class BattleRewardService : IRewardService
{
    [Inject] private SignalBus _signalBus;

    [Inject] private IStandbyZone _standbyZone;
    [Inject] private IHandService _handService;
    [Inject] private ICardSpawner _cardSpawner;

    [Inject] private IResourceRewardStrategy _rewardStrategy;
    [Inject] private IResourceManager _resourceService;

    [Inject] private ICombatModifierService _combatModifier;

    #region 卡牌選擇功能
    public IEnumerable<ChessPiece> GetAvailableRewards()
    {
        if (_combatModifier != null && _combatModifier.GetDropMultiplier() <= 0f)
        {
            Debug.Log("[BattleRewardService] 焦土戰術生效，本次無卡牌掉落！");
            return Enumerable.Empty<ChessPiece>();
        }

        return _standbyZone.GetStandbyPieces(Team.White);
    }

    public List<RewardUIData> GetRewardUIData()
    {
        // 將 Domain 物件映射為 UI 用的 DTO
        return GetAvailableRewards().Select(piece => new RewardUIData
        {
            Id = piece.Id,
            DisplayName = piece.Type.ToString(), // 或從 Config 拿更友善的名字
            Icon = null, // 從 Config 獲取對應 Sprite
            IsBroken = piece.IsBroken,
            SupplyCost = 0 // 依需求計算
        }).ToList();
    }

    public void ClaimCardReward(PieceID pieceId)
    {
        // 1. 從待命區找到數據
        var pieces = _standbyZone.GetStandbyPieces(Team.White);
        var piece = pieces.FirstOrDefault(p => p.Id == pieceId);

        if (piece != null)
        {
            // 2. 轉化為卡牌並加入手牌
            var card = _cardSpawner.SpawnCardFromPiece(piece, GameMode.Mode3D);
            _handService.AddCard(card);

            // 3. 從待命區移除
            _standbyZone.RemovePiece(pieceId);
            Debug.Log($"[Service] 棋子 {pieceId} 已轉化為卡牌");
        }
    }
    #endregion

    #region  資源獎勵功能
    // 暫存當次戰鬥的資源獎勵
    private ResourceRewardData? _currentResourceReward;

    public ResourceRewardData GenerateResourceRewards()
    {
        // 1. 交給外部注入的「策略」來產生【基礎獎勵】
        var baseReward = _rewardStrategy.GenerateReward();

        // 2. 攔截：向修改器詢問目前的【掉落倍率】
        // (補給掠奪會回傳 2.0，焦土戰術會回傳 0.0，正常為 1.0)
        float multiplier = _combatModifier != null ? _combatModifier.GetDropMultiplier() : 1f;

        if (multiplier != 1f)
        {
            Debug.Log($"[BattleRewardService] 戰鬥修改器生效！當前掉落倍率: {multiplier}x");
        }

        // 3. 套用倍率並覆蓋當前獎勵
        _currentResourceReward = new ResourceRewardData
        {
            SupplyAmount = baseReward.SupplyAmount * multiplier,
            CoinAmount = baseReward.CoinAmount * multiplier,
            MoraleAmount = baseReward.MoraleAmount * multiplier
        };

        return _currentResourceReward.Value;
    }

    public void ClaimResourceRewards()
    {
        if (_currentResourceReward.HasValue)
        {
            var reward = _currentResourceReward.Value;
            if (reward.SupplyAmount > 0) _resourceService.ReplenishSupply(reward.SupplyAmount);
            if (reward.CoinAmount > 0) _resourceService.RewardCoin(reward.CoinAmount);
            if (reward.MoraleAmount > 0) _resourceService.IncreaseeMorale(reward.MoraleAmount);

            _currentResourceReward = null;
        }
    }
    #endregion
}
