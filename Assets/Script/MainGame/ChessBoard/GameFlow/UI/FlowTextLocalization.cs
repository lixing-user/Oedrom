using System.Collections.Generic;
using UnityEngine;

public class FlowTextLocalization
{
    private Dictionary<string, string> _texts = new Dictionary<string, string>
    {
        // 回合相關
        ["your_turn"] = "你的回合",
        ["enemy_turn"] = "敵方回合",
        ["player_turn"] = "玩家回合",
        ["game_start"] = "遊戲開始！",

        // 階段相關
        ["phase_standby"] = "準備階段",
        ["phase_main1"] = "主要階段 I",
        ["phase_movement"] = "移動階段",
        ["phase_main2"] = "主要階段 II",
        ["phase_resolution"] = "結算階段",
        ["phase_end"] = "結束階段",

        // 階段提示
        ["hint_standby"] = "補給資源、抽取卡牌",
        ["hint_main1"] = "放置卡牌",
        ["hint_movement"] = "移動棋子",
        ["hint_main2"] = "攻擊敵方、觸發事件",
        ["hint_resolution"] = "處理戰鬥結果",
        ["hint_end"] = "回合結束"
    };

    public string GetText(string key)
    {
        return _texts.TryGetValue(key, out string value) ? value : key;
    }

    public string GetTurnText(Team team)
    {
        return team == Team.White
            ? GetText("player_turn")
            : GetText("enemy_turn");
    }

    public string GetFlowText(GameFlow flow)
    {
        string key = flow switch
        {
            GameFlow.Setup => "game_setup",
            GameFlow.PlayerTurn => "your_turn",
            GameFlow.EnemyTurn => "enemy_turn",
            GameFlow.Resolution => "resolution",
            GameFlow.GameEnd => "",
            _ => "unknown"
        };
        return GetText(key);
    }

    public string GetPhaseText(GamePhaseState phase)
    {
        string key = phase switch
        {
            GamePhaseState.StandbyPhase => "phase_standby",
            GamePhaseState.MainPhase1 => "phase_main1",
            GamePhaseState.MovementPhase => "phase_movement",
            GamePhaseState.MainPhase2 => "phase_main2",
            GamePhaseState.ResolutionPhase => "phase_resolution",
            GamePhaseState.EndPhase => "phase_end",
            _ => "unknown"
        };
        return GetText(key);
    }

    public string GetPhaseHint(GamePhaseState phase)
    {
        string key = phase switch
        {
            GamePhaseState.StandbyPhase => "hint_standby",
            GamePhaseState.MainPhase1 => "hint_main1",
            GamePhaseState.MovementPhase => "hint_movement",
            GamePhaseState.MainPhase2 => "hint_main2",
            GamePhaseState.ResolutionPhase => "hint_resolution",
            GamePhaseState.EndPhase => "hint_end",
            _ => null
        };
        return key != null ? GetText(key) : null;
    }
}
