using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDifficultyConfig", menuName = "Scriptable Objects/EnemyDifficultyConfig")]
public class EnemyDifficultyConfig : ScriptableObject
{
    // 可以將門檻與資料封裝在一起
    [System.Serializable]
    public class DifficultyLevel
    {
        public int requiredBattleCount; // 觸發門檻條件之一
        public EnemyDifficultyData difficultyData;
    }

    public List<DifficultyLevel> difficultyLevels;
}