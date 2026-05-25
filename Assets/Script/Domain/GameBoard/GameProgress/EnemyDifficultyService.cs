using System.Linq;
using UnityEngine;
using Zenject;

// Domain/Services/EnemyDifficultyService.cs
public class EnemyDifficultyService : IEnemyDifficultyService
{
    [Inject] private readonly RunProgress _runProgress;
    [Inject] private readonly PlayerResource _playerResource;

    [Inject] private readonly EnemyDifficultyConfig _config;

    public (int levelIndex, EnemyDifficultyData data) GetCurrentDifficulty()
    {
        int currentBattles = _runProgress.CompletedBattles;
        float currentMorale = _playerResource.Morale;

        // 假設 config 中的 difficultyLevels 已經照難度由低到高排序
        // 找到符合「戰鬥次數門檻」的最高難度
        var applicableDifficulty = _config.difficultyLevels
            .Where(level => currentBattles >= level.requiredBattleCount)
            .LastOrDefault();

        int targetIndex = _config.difficultyLevels.IndexOf(applicableDifficulty);

        //士氣（Morale）機制：如果士氣較高，稍微降低難度
        if (currentMorale > 4f && targetIndex > 0)
        {
            targetIndex -= 1;
            applicableDifficulty = _config.difficultyLevels[targetIndex];
        }

        // 防呆機制：如果找不到，回傳最基礎的難度
        if (applicableDifficulty == null || targetIndex < 0)
        {
            return (0, _config.difficultyLevels[0].difficultyData);
        }

        // 同時回傳「難度層級 Index」與「難度資料」
        return (targetIndex, applicableDifficulty.difficultyData);
    }
}

public interface IEnemyDifficultyService
{
    (int levelIndex, EnemyDifficultyData data) GetCurrentDifficulty();
}