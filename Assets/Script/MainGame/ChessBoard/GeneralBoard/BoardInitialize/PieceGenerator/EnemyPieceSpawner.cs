using System;
using UnityEngine;
using Zenject;

public class EnemyPieceSpawner : IInitializable, IDisposable
{
    [Header("所有可用難度資料")]
    public EnemyDifficultyData EasyDifficulty;
    public EnemyDifficultyData NormalDifficulty;
    public EnemyDifficultyData HardDifficulty;
    public EnemyDifficultyData HardCoreDifficulty;

    private EnemyDifficultyData difficultyData;
    private SignalBus signalBus;

    public EnemyPieceSpawner(EnemyDifficultyData difficultyData, SignalBus signalBus)
    {
        this.difficultyData = difficultyData;
        this.signalBus = signalBus;
    }

    public void Initialize()
    {
        signalBus.Subscribe<EnemyDifficultyDeterminedSignal>(OnDifficultyChanged);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<EnemyDifficultyDeterminedSignal>(OnDifficultyChanged);
    }

    private void OnDifficultyChanged(EnemyDifficultyDeterminedSignal signal)
    {
        int baseThresholdHardCore = 1;
        int baseThresholdHard = 3;
        int baseThresholdNormal = 7;

        int bonus = 0; //(BattleManager.BattleCount / 3) * 2;

        if (signal.Popularity <= baseThresholdHardCore + bonus)
            difficultyData = HardCoreDifficulty;
        else if (signal.Popularity <= baseThresholdHard + bonus)
            difficultyData = HardDifficulty;
        else if (signal.Popularity <= baseThresholdNormal + bonus)
            difficultyData = NormalDifficulty;
        else
            difficultyData = EasyDifficulty;

        RespawnEnemies();
    }

    public void RespawnEnemies()
    {
        // 根據 difficultyData.enemyPieceTypes 與 maxEnemyCount 產生敵人
        // 具體生成棋子邏輯
    }
}
