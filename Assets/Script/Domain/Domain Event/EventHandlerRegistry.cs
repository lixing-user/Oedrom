using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Interfaces
// 泛型事件處理器介面
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    void Handle(TEvent domainEvent);
}


// 事件處理器註冊表
public interface IEventHandlerRegistry
{
    void Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IDomainEvent;
    void Unregister<TEvent>(IEventHandler<TEvent> handler) where TEvent : IDomainEvent;
    IEnumerable<IEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : IDomainEvent;
}
#endregion


#region Implementations
public class EventHandlerRegistry : IEventHandlerRegistry
{
    private readonly Dictionary<Type, List<object>> _handlers = new();


    public void Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<object>();
        }

        _handlers[eventType].Add(handler);
        Debug.Log($"[Registry] 註冊處理器: {handler.GetType().Name} for {eventType.Name}");
    }


    public void Unregister<TEvent>(IEventHandler<TEvent> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.ContainsKey(eventType))
        {
            _handlers[eventType].Remove(handler);
        }
    }


    public IEnumerable<IEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.ContainsKey(eventType))
        {
            return _handlers[eventType].Cast<IEventHandler<TEvent>>();
        }
        return Enumerable.Empty<IEventHandler<TEvent>>();
    }
}
#endregion