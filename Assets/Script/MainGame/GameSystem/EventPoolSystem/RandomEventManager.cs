using System.Collections.Generic;
using System;
using UnityEngine;
using Zenject;


// Application/RandomEventManager.cs
public class RandomEventManager : IInitializable, IRandomEventManager
{
    [InjectOptional] private readonly ITutorialOverrideService _overrideService;

    private readonly SignalBus _signalBus;
    private readonly EventEffectDispatcher _effectDispatcher;
    private readonly EventPoolConfig _eventPoolConfig;
    private readonly EffectContextFactory _contextFactory;

    public RandomEventManager(
        EventEffectDispatcher effectDispatcher,
        SignalBus signalBus,
        EventPoolConfig eventPoolConfig,
        EffectContextFactory contextFactory)
    {
        _effectDispatcher = effectDispatcher;
        _signalBus = signalBus;
        _eventPoolConfig = eventPoolConfig;
        _contextFactory = contextFactory;
    }


    private List<RandomEventData> _globalEvents = new List<RandomEventData>();
    private Dictionary<TerrainType, List<RandomEventData>> _terrainEvents =
        new Dictionary<TerrainType, List<RandomEventData>>();


    public void Initialize()
    {
        InitializeEventPool();
    }

    private void InitializeEventPool()
    {
        if (_eventPoolConfig == null)
        {
            Debug.LogError("[RandomEventManager] EventPoolConfig 未設定！");
            return;
        }

        foreach (var eventData in _eventPoolConfig.allEvents)
        {
            if (eventData.eventType == RandomEventType.Neutral)
            {
                _globalEvents.Add(eventData);
            }
            if (eventData.eventType == RandomEventType.Ally)
            {
                _globalEvents.Add(eventData);
            }
            // 可以根據其他條件分類事件...
        }

        Debug.Log($"[RandomEventManager] 載入 {_globalEvents.Count} 個全域事件");
    }


    // Step 1: 抽籤並發送 UI 信號 (不執行實際數值變化)
    public RandomEventData DraftAndShowEvent(PieceID triggerPieceId)
    {
        if (_globalEvents.Count == 0) return null;

        // 1. 【攔截點】：檢查是否有保底的主線/教學事件
        RandomEventData selectedEvent = null;

        if (_overrideService != null && _overrideService.TryGetForcedEvent(out RandomEventData forcedEvent))
        {
            if (forcedEvent == null)
            {
                Debug.Log("[RandomEventManager] 教學/主線強制【壓制】了隨機事件，本次無事發生。");
                return null; // 直接回傳 null，交由 MainSecPhase 跳過等待
            }

            Debug.Log($"[RandomEventManager] 觸發教學/主線強制事件: {forcedEvent.eventName}");
            // 直接走觸發流程，跳過隨機抽籤
            selectedEvent = forcedEvent;
        }
        else
        {
            // 2. 如果沒有強制事件，才使用權重隨機選擇事件
            var checkContext = _contextFactory.Create(triggerPieceId);
            var availableEvents = _globalEvents.FindAll(e => e.CheckConditions(checkContext));

            selectedEvent = SelectWeightedRandom(availableEvents);
        }

        var optionDTOs = new List<EventOptionDTO>();
        if (selectedEvent.options != null)
        {
            for (int i = 0; i < selectedEvent.options.Count; i++)
            {
                optionDTOs.Add(new EventOptionDTO
                {
                    OptionIndex = i,
                    OptionName = selectedEvent.options[i].optionName,
                    Description = selectedEvent.options[i].description
                });
            }
        }

        // 3. 發送 Signal（用於 UI 顯示）
        _signalBus.Fire(new RandomEventTriggeredSignal(
            selectedEvent.eventId,
            selectedEvent.eventName,
            selectedEvent.description,
            selectedEvent.eventIcon,
            optionDTOs));

        // 回傳抽中結果給 Facade
        return selectedEvent;
    }
    public void ResolvePendingEvent(RandomEventData eventData, int selectedOptionIndex, PieceID targetPieceId)
    {
        if (eventData != null && selectedOptionIndex >= 0 && selectedOptionIndex < eventData.options.Count)
        {
            var selectedOption = eventData.options[selectedOptionIndex];
            Debug.Log($"[RandomEventManager] 玩家選擇了選項: {selectedOption.optionName}");

            _effectDispatcher.Dispatch(selectedOption.effects, targetPieceId);
        }
        else
        {
            Debug.LogError("[RandomEventManager] 無效的事件或選項索引！");
        }
    }


    public void TriggerTerrainEvent(TerrainType currentTerrain)
    {
        //List<Action> candidateEvents = new List<Action>();


        //// 加入地形專屬事件（可重複多次以增加權重）
        //if (terrainEventDict.ContainsKey(currentTerrain))
        //{
        //    candidateEvents.AddRange(terrainEventDict[currentTerrain]);
        //    candidateEvents.AddRange(terrainEventDict[currentTerrain]); // 例如再加一次，提升權重
        //}

        //// 加入全域事件
        //candidateEvents.AddRange(eventList);

        //if (candidateEvents.Count == 0) return;

        //int randomIndex = UnityEngine.Random.Range(0, candidateEvents.Count);
        //candidateEvents[randomIndex].Invoke();
    }


    /// <summary>
    /// 加權隨機選擇
    /// </summary>
    private RandomEventData SelectWeightedRandom(List<RandomEventData> events)
    {
        int totalWeight = 0;
        foreach (var e in events)
        {
            totalWeight += e.weight;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var e in events)
        {
            cumulativeWeight += e.weight;
            if (randomValue < cumulativeWeight)
            {
                return e;
            }
        }

        return events[0]; // 容錯
    }
}

public interface IRandomEventManager
{
    RandomEventData DraftAndShowEvent(PieceID triggerPieceId);
    void ResolvePendingEvent(RandomEventData eventData, int selectedOptionIndex, PieceID targetPieceId);
    void TriggerTerrainEvent(TerrainType currentTerrain);
}