using System;
using UnityEngine;
using Zenject;

public class BattleResolveSignal 
{
    public PieceID AttackerId { get; }
    public PieceID DefenderId { get; }
    public ChessPieceType AttackerType { get; set; }       // 攻擊方棋子類型
    public ChessPieceType DefenderType { get; set; }       // 防守方棋子類型
    public Team AttackerTeam { get; set; }                  // 攻擊方隊伍 (0 或 1)
    public Team DefenderTeam { get; set; }                  // 防守方隊伍
    public PiecePosition AttackerPos { get; set; }            // 攻擊方位置
    public PiecePosition DefenderPos { get; set; }            // 防守方位置
    public int WinnerTeam { get; set; }                    // 勝利隊伍 (-1 平手)
    public BattleType BattleType { get; set; } = BattleType.Normal;     // Session 管理層需要的欄位

    public BattleResolveSignal(PieceID attackerId, PieceID defenderId)
    {
        AttackerId = attackerId ?? throw new ArgumentNullException(nameof(attackerId));
        DefenderId = defenderId ?? throw new ArgumentNullException(nameof(defenderId));
    }
}
