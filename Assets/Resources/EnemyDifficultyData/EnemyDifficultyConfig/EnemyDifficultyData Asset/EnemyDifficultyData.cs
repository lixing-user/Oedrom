using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDifficultyData", menuName = "Scriptable Objects/EnemyDifficultyData")]
public class EnemyDifficultyData : ScriptableObject
{
    public List<ChessPieceType> enemyPieceTypes;
    public int maxEnemyCount;
}
