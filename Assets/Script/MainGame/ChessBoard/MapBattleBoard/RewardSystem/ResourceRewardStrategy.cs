using UnityEngine;
using Zenject;

// Domain/Strategies/IResourceRewardStrategy.cs
public interface IResourceRewardStrategy
{
    /// <summary>
    /// 計算並產生當次的資源獎勵
    /// </summary>
    ResourceRewardData GenerateReward();
}

#region Normal Battle Session
public class DifficultyBasedRewardStrategy : IResourceRewardStrategy
{
    [Inject] private IEnemyDifficultyService _difficultyService;

    public ResourceRewardData GenerateReward()
    {
        var difficultyResult = _difficultyService.GetCurrentDifficulty();
        int tierIndex = difficultyResult.levelIndex;

        float supply = UnityEngine.Random.Range(10f, 30f);
        float coin = UnityEngine.Random.Range(100f, 500f);
        float morale = (tierIndex >= 2) ? 1f : 0f; // Hard 以上給士氣

        return new ResourceRewardData { SupplyAmount = supply, CoinAmount = coin, MoraleAmount = morale };
    }
}
#endregion

#region Chase Battle Session
public class RandomRewardStrategy : IResourceRewardStrategy
{
    public ResourceRewardData GenerateReward()
    {
        return new ResourceRewardData
        {
            SupplyAmount = UnityEngine.Random.Range(20f, 50f),
            CoinAmount = UnityEngine.Random.Range(200f, 800f),
            MoraleAmount = 0f
        };
    }
}
#endregion