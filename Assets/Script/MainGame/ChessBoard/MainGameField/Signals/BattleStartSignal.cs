using System;
using UnityEngine;
using Zenject;

// 於MainGameField場景中，當棋子觸發戰鬥時發送此信號
public class BattleStartSignal 
{
    public PieceID AttackerId { get; }
    public PieceID DefenderId { get; }

    public ChessPieceType AttackerType { get; set; }       // 攻擊方棋子類型
    public ChessPieceType DefenderType { get; set; }       // 防守方棋子類型

    public PiecePosition AttackerPos { get; set; }            // 攻擊方位置
    public PiecePosition DefenderPos { get; set; }            // 防守方位置

    public Team AttackerTeam { get; set; }                  // 攻擊方隊伍 (0 或 1)
    public Team DefenderTeam { get; set; }                  // 防守方隊伍


    public BattleStartSignal(
         PieceID attackerId,
         PieceID defenderId,
         PiecePosition from,
         PiecePosition to)
    {
        AttackerId = attackerId ?? throw new ArgumentNullException(nameof(attackerId));
        DefenderId = defenderId ?? throw new ArgumentNullException(nameof(defenderId));
        AttackerPos = from ?? throw new ArgumentNullException(nameof(from));
        DefenderPos = to ?? throw new ArgumentNullException(nameof(to));
    }

    public static BattleStartSignal Create(
       ChessPiece attacker,
       ChessPiece defender,
       PiecePosition from,
       PiecePosition to)
    {
        return new BattleStartSignal(
            attacker.Id,
            defender.Id,
            from,
            to
        );
    }
}