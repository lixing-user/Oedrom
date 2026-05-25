using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public struct QuestDisplayData
{
    public string QuestId;
    public QuestType Type;
    public string TitleText;     // 已經處理好的標題字串
    public string ObjectiveText; // 已經處理好的目標說明字串
}


public class QuestTrackerPresenter : IInitializable, IDisposable
{
    [Inject] private readonly QuestManager _questManager;
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IQuestTrackerView _view;

    public void Initialize()
    {
        _signalBus.Subscribe<QuestStateChangedSignal>(UpdateView);
        UpdateView(); // 初始化時刷新一次
    }

    public void Dispose()
    {
        _signalBus?.Unsubscribe<QuestStateChangedSignal>(UpdateView);
    }


    // 核心邏輯：向 Manager 拿資料，翻譯後餵給 View
    private void UpdateView()
    {
        var activeQuests = _questManager.ActiveQuests;

        // 1. 處理主線
        var mainQuest = activeQuests
            .OrderBy(q => q.questType)
            .FirstOrDefault(q => q.questType == QuestType.StoryMain || q.questType == QuestType.ExplorationMain);

        if (mainQuest != null)
        {
            string title = $"[{(mainQuest.questType == QuestType.StoryMain ? "主線" : "探索")}] {mainQuest.questId}";
            string objective = FormatConditions(mainQuest.completionConditions);
            _view.SetMainQuestDisplay(true, title, objective);
        }
        else
        {
            _view.SetMainQuestDisplay(true, "[自由探索中]", "在棋盤世界中尋找異常點或前進線索。");
        }
    }

    // 單一職責：專門把 ConditionSO 翻譯成人類語言
    private string FormatConditions(MacroConditionSO[] conditions)
    {
        if (conditions == null || conditions.Length == 0) return "目標：無限制。";

        var descList = conditions.Select(cond =>
        {
            if (cond is TutorialSequenceCompletedCondition seq) return $"完成序列：{seq.SequenceId}";
            if (cond is FlagMetCondition flag) return $"達成標記：{flag.requiredFlag}";
            return "達成特定條件";
        });

        return "目標：" + string.Join(" 且 ", descList);
    }
}
