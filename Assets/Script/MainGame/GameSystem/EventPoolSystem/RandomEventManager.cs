using System.Collections.Generic;
using System;
using UnityEngine;
using Zenject;


// Application/RandomEventManager.cs
public class RandomEventManager : IInitializable, IRandomEventManager
{
    private readonly ITutorialOverrideService _overrideService;

    private readonly SignalBus _signalBus;
    private readonly EventEffectDispatcher _effectDispatcher;
    private readonly EventPoolConfig _eventPoolConfig;
    private readonly EffectContextFactory _contextFactory;

    public RandomEventManager(
        EventEffectDispatcher effectDispatcher,
        SignalBus signalBus,
        EventPoolConfig eventPoolConfig,
        EffectContextFactory contextFactory,
        [InjectOptional] ITutorialOverrideService overrideService)
    {
        _effectDispatcher = effectDispatcher;
        _signalBus = signalBus;
        _eventPoolConfig = eventPoolConfig;
        _contextFactory = contextFactory;
        _overrideService = overrideService;
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
    public RandomEventData DraftEvent(PieceID triggerPieceId)
    {
        if (_overrideService != null && _overrideService.IsRandomEventSuppressed)
            return null;

        if (_overrideService != null && _overrideService.TryGetForcedEvent(out var forced))
            return forced; // null = 本次不發生事件，這裡不發任何 signal

        if (_globalEvents.Count == 0) return null;

        var checkContext = _contextFactory.Create(triggerPieceId);
        var availableEvents = _globalEvents.FindAll(e => e.CheckConditions(checkContext));
        if (availableEvents.Count == 0) return null;

        Debug.Log($"[Manager] 檢查的鎖 ID: {_overrideService.GetHashCode()}");

        return SelectWeightedRandom(availableEvents);
    }

    public void ShowEvent(RandomEventData selectedEvent)
    {
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
    RandomEventData DraftEvent(PieceID triggerPieceId);
    void ShowEvent(RandomEventData selectedEvent);
    void ResolvePendingEvent(RandomEventData eventData, int selectedOptionIndex, PieceID targetPieceId);
    void TriggerTerrainEvent(TerrainType currentTerrain);
}