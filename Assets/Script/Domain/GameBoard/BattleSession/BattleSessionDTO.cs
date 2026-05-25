using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;

// Application / Battle / DTOs / BattleSessionData.cs

/// <summary>
/// 戰鬥會話數據（進入戰鬥前保存）
/// </summary>
public class BattleSessionData
{
    // 共用資料 
    public string BattleId { get; set; }
    public BattleType BattleType { get; set; }
    public ChessPieceType AttackerType { get; set; }
    public ChessPieceType DefenderType { get; set; }
    public Team AttackerTeam { get; set; }
    public Team DefenderTeam { get; set; }
    public List<PieceDTO> InitialBoardSnapshot { get; set; }
    public List<CardDTO> InitialHandSnapshot { get; set; }

    // 棋盤戰鬥專用
    public PieceID AttackerId { get; set; }
    public PieceID DefenderId { get; set; }
    public PiecePosition AttackerPos { get; set; }
    public PiecePosition DefenderPos { get; set; }

    // 搶劫戰鬥專用
    public HandCard AttackerCard { get; set; }
    public List<string> TargetItemId { get; set; }

    /// <summary>
    /// 從 Domain 模型創建 DTO
    /// </summary>
    public static BattleSessionData FromDomain(Battle battle, BattleSessionData previousData = null)
    {
        return new BattleSessionData
        {
            BattleId = battle.Id.Value,
            AttackerId = battle.Attacker.Id,
            DefenderId = battle.Defender.Id,
            AttackerType = battle.Attacker.Type,
            DefenderType = battle.Defender.Type,
            AttackerPos = battle.Attacker.CurrentPosition,
            DefenderPos = battle.Defender.CurrentPosition,
            AttackerTeam = battle.Attacker.Team,
            DefenderTeam = battle.Defender.Team,
            AttackerCard = previousData?.AttackerCard,
            TargetItemId = previousData?.TargetItemId ?? new List<string>()
        };
    }
}


/// <summary>
/// 戰鬥結果數據（戰鬥場景結束時保存）
/// </summary>
public class BattleResultData
{
    public string BattleId { get; set; }
    public int WinnerTeam { get; set; }  // -1 表示平手

    public static BattleResultData FromBattleContext(BattleSessionData session, Team? winner)
    {
        return new BattleResultData
        {
            BattleId = session.BattleId,
            WinnerTeam = winner.HasValue ? (int)winner.Value : -1
        };
    }
}