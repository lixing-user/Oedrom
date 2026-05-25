using System.Collections.Generic;
using UnityEngine;


// 觸發隨機事件
public class RandomEventTriggeredSignal
{
    public string EventId { get; }
    public string EventName { get; }
    public string Description { get; }
    public Sprite Icon { get; }
    public IReadOnlyList<EventOptionDTO> Options { get; }

    public RandomEventTriggeredSignal(
        string eventId, 
        string eventName, 
        string description, 
        Sprite icon, 
        IReadOnlyList<EventOptionDTO> options)
    {
        EventId = eventId;
        EventName = eventName;
        Description = description;
        Icon = icon;
        Options = options;
    }
}

// 讓presentation層級的程式碼發送，推進遊戲進程
public class RandomEventConfirmedSignal
{
    public int SelectedOptionIndex { get; }

    public RandomEventConfirmedSignal(int selectedIndex)
    {
        SelectedOptionIndex = selectedIndex;
    }
}