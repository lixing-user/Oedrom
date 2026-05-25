using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class QuestManager : IInitializable, IDisposable
{
    [Inject] private SignalBus _signalBus;
    [Inject] private QuestDatabaseSO _database;
    [Inject] private DiContainer _container;

    [Inject] private ProjectGameActionContext _actionContext;

    // 狀態追蹤 (未來可整合至 SaveSystem)
    private HashSet<string> _completedQuestIds = new HashSet<string>();
    private List<QuestSO> _activeQuests = new List<QuestSO>();

    public IReadOnlyList<QuestSO> ActiveQuests => _activeQuests;    // 將唯讀清單公開

    // 任務系統是否被禁用
    private bool _isQuestSystemDisabled = false;


    public void Initialize()
    {
        // 訂閱推進遊戲狀態的關鍵信號
        _signalBus.Subscribe<StartGameCommandSignal>(OnStartGameIntentReceived);

        _signalBus.Subscribe<GameSessionStateChangedSignal>(OnSessionStateChanged);

        _signalBus.Subscribe<GameFlagAddedSignal>(OnGameStateChanged);
        _signalBus.Subscribe<StoryChapterCompletedSignal>(OnGameStateChanged);
        _signalBus.Subscribe<BattleResolveSignal>(OnGameStateChanged);

        _signalBus.Subscribe<SceneTransitionCompletedSignal>(x => OnGameStateChanged());
        _signalBus.Subscribe<ChangeBattleTypeSignal>(x => OnGameStateChanged());
        _signalBus.Subscribe<FlowChangedSignal>(x => OnGameStateChanged());
        _signalBus.Subscribe<GameEndedSignal>(x => OnGameStateChanged());

        LoadProgress();
    }

    public void Dispose()
    {
        _signalBus?.Unsubscribe<StartGameCommandSignal>(OnStartGameIntentReceived);
        _signalBus?.Unsubscribe<GameFlagAddedSignal>(OnGameStateChanged);
        _signalBus?.Unsubscribe<StoryChapterCompletedSignal>(OnGameStateChanged);
        _signalBus?.Unsubscribe<BattleResolveSignal>(OnGameStateChanged);
        _signalBus?.Unsubscribe<GameSessionStateChangedSignal>(OnSessionStateChanged);
        _signalBus?.Unsubscribe<SceneTransitionCompletedSignal>(x => OnGameStateChanged());
        _signalBus?.Unsubscribe<ChangeBattleTypeSignal>(x => OnGameStateChanged());
        _signalBus?.Unsubscribe<FlowChangedSignal>(x => OnGameStateChanged());
        _signalBus?.Unsubscribe<GameEndedSignal>(x => OnGameStateChanged());
    }

    #region 事件處理
    private void OnStartGameIntentReceived(StartGameCommandSignal signal)
    {
        if (signal.Mode == GameStartMode.DebugStart)
        {
            Debug.Log("[QuestManager] 偵測到新遊戲，清空所有任務暫存狀態");
            _isQuestSystemDisabled = true;
            _completedQuestIds.Clear();
            _activeQuests.Clear();
            return;
        }

        if (signal.Mode == GameStartMode.NewGame)
        {
            Debug.Log("[QuestManager] 偵測到新遊戲，清空所有任務暫存狀態");
            _isQuestSystemDisabled = false;
            _completedQuestIds.Clear();
            _activeQuests.Clear();

            // 如果 EvaluateQuests 是被其他地方同步觸發的，這裡清空後就可以正常接上判定了
        }
        else if (signal.Mode == GameStartMode.LoadGame)
        {
            _isQuestSystemDisabled = false; // 讀檔也要確保開啟
        }
    }

    private void OnGameStateChanged()
    {
        EvaluateQuests();
    }

    private void OnSessionStateChanged(GameSessionStateChangedSignal signal)
    {
        if (signal.NextState == GameSessionState.MainMenu) return;

        if (_isQuestSystemDisabled) return;

        EvaluateQuests();
    }

    private void EvaluateQuests()
    {
        if (_isQuestSystemDisabled) return;

        #region 階段一：優先處理【主線任務】
        // 1A. 檢查進行中的任務是否完成
        for (int i = _activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = _activeQuests[i];
            if (AreConditionsMet(quest.completionConditions))
            {
                CompleteQuest(quest);
                return;// 狀態改變後立刻 return，因為 CompleteQuest 會重新呼叫 EvaluateQuests
            }

            Debug.Log($"[QuestManager] 正在評估任務解鎖條件: {quest.questId}");
        }

        // 1B. 檢查未觸發的主線是否滿足解鎖條件
        foreach (var quest in _database.allQuests)
        {
            //if (!quest.isMainQuest) continue;
            if (quest.questType == QuestType.Side) continue;
            if (_completedQuestIds.Contains(quest.questId) || _activeQuests.Contains(quest))
                continue;

            if (AreConditionsMet(quest.unlockConditions))
            {
                ActivateQuest(quest);
                return; // 每次只觸發一個任務，完成後再評估一次，確保主線優先
            }

            Debug.Log($"[QuestManager] 正在評估任務解鎖條件: {quest.questId}");
        }
        #endregion

        // 階段二：判定是否處於【主線強制接管狀態】
        //bool isMainQuestActive = _activeQuests.Any(q => q.isMainQuest);
        bool isMainQuestActive = _activeQuests.Any(q => q.questType == QuestType.StoryMain);
        Debug.Log($"<color=cyan>[QuestManager] 狀態更新！目前是否有 StoryMain: {isMainQuestActive}。準備進入階段三支線判定...</color>");

        #region 階段三：處理【支線任務】 (背景監聽模式)
        // 3A. 檢查支線是否完成 (即使主線進行中，也允許支線在背景完成)
        for (int i = _activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = _activeQuests[i];
            //if (!quest.isMainQuest && AreConditionsMet(quest.completionConditions))
            if (quest.questType == QuestType.Side && AreConditionsMet(quest.completionConditions))
            {
                CompleteQuest(quest);
                return;
            }
        }

        // 3B. 檢查支線是否解鎖 (主線進行中時，封鎖新支線的觸發)
        if (!isMainQuestActive)
        {
            foreach (var quest in _database.allQuests)
            {
                //if (quest.isMainQuest) continue;
                if (quest.questType != QuestType.Side) continue;
                if (_completedQuestIds.Contains(quest.questId) || _activeQuests.Contains(quest)) continue;

                Debug.Log($"<color=orange>[QuestManager] 階段三：正在評估【支線】解鎖條件 - {quest.questId}</color>");

                if (AreConditionsMet(quest.unlockConditions))
                {
                    ActivateQuest(quest);
                    Debug.Log($"<color=green>[QuestManager] 【支線】條件全部滿足！準備觸發 - {quest.questId}</color>");
                    return;
                }
                else
                {
                    Debug.Log($"<color=red>[QuestManager] 【支線】{quest.questId} 的條件尚未滿足。</color>");
                }
            }
        }
        #endregion

    }
    #endregion

    private void ActivateQuest(QuestSO quest)
    {
        Debug.Log($"[QuestManager] 任務觸發: {quest.questId}");
        _activeQuests.Add(quest);

        ExecuteActions(quest.actionsOnStart);
        EvaluateQuests(); // 觸發後可能立刻滿足完成條件，需再評估一次

        _signalBus.Fire<QuestStateChangedSignal>();
    }

    private void CompleteQuest(QuestSO quest)
    {
        Debug.Log($"[QuestManager] 任務完成: {quest.questId}");
        _activeQuests.Remove(quest);
        _completedQuestIds.Add(quest.questId);

        // 【修正】1. 先執行該任務完成時該做的動作 (包含切換場景、拉起封鎖線)
        ExecuteActions(quest.actionsOnComplete);

        // 【修正】2. 再標記完成，發送 Signal 讓下一個 Quest 有機會觸發
        _actionContext.TutorialProgressService?.MarkStepCompleted($"QuestDone_{quest.questId}");

        SaveProgress();
        EvaluateQuests(); // 完成後可能解鎖新任務，需再評估一次

        _signalBus.Fire<QuestStateChangedSignal>();
    }

    private bool AreConditionsMet(MacroConditionSO[] conditions)
    {
        if (conditions == null || conditions.Length == 0) return true;

        foreach (var cond in conditions)
        {
            if (cond == null) continue;
            if (!cond.IsConditionMet(_actionContext)) return false;
        }
        return true;
    }

    private void ExecuteActions(MacroActionSO[] actions)
    {
        if (actions == null) return;
        foreach (var action in actions)
        {
            if (action == null)
            {
                Debug.LogWarning("[QuestManager] 發現空的 Action 欄位，已自動跳過。請檢查 QuestSO 的 Inspector 設定！");
                continue;
            }
            action.Execute(_actionContext);
        }
    }

    #region 進度保存與讀取
    private void LoadProgress()
    {
        //_completedQuestIds.Clear();
        //_activeQuests.Clear();

        //// 1. 恢復已完成的任務
        //string compStr = PlayerPrefs.GetString("Quest_Completed", string.Empty);
        //if (!string.IsNullOrEmpty(compStr))
        //{
        //    foreach (var id in compStr.Split(','))
        //    {
        //        _completedQuestIds.Add(id);
        //    }
        //}

        //// 2. 恢復正在進行中的任務 (這點非常重要！)
        //string actStr = PlayerPrefs.GetString("Quest_Active", string.Empty);
        //if (!string.IsNullOrEmpty(actStr))
        //{
        //    var activeIds = new HashSet<string>(actStr.Split(','));
        //    foreach (var quest in _database.allQuests)
        //    {
        //        if (activeIds.Contains(quest.questId))
        //        {
        //            _activeQuests.Add(quest);

        //            // 關鍵核心：
        //            // 因為玩家是中途讀檔進來的，我們必須重新執行一次任務的「開始動作」
        //            // 例如：鎖定路由 (LockRouting)、生成特定怪物、重設 UI 高光等，確保場景狀態與任務吻合
        //            ExecuteActions(quest.actionsOnStart);
        //        }
        //    }
        //}
    }

    private void SaveProgress() 
    {
        //// 儲存已完成的任務 ID (例如 "Quest_01,Quest_02")
        //PlayerPrefs.SetString("Quest_Completed", string.Join(",", _completedQuestIds));

        //// 儲存正在進行中的任務 ID
        //PlayerPrefs.SetString("Quest_Active", string.Join(",", _activeQuests.Select(q => q.questId)));

        //PlayerPrefs.Save();
    }
    #endregion
}

public struct LockRoutingSignal { }
public struct UnlockRoutingSignal { }

public struct QuestStateChangedSignal { }
