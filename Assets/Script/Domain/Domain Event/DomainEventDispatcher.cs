using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IDomainEventDispatcher
{
    void Dispatch(IEnumerable<IDomainEvent> events);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly SignalBus _signalBus;
    private readonly IEventHandlerRegistry _registry;

    public DomainEventDispatcher(SignalBus signalBus, IEventHandlerRegistry registry)
    {
        _signalBus = signalBus;
        _registry = registry;
    }

    public void Dispatch(IEnumerable<IDomainEvent> events)
    {
        var eventsToDispatch = events.ToList();

        foreach (var domainEvent in eventsToDispatch)
        {
            Debug.Log($"Domain Event Dispatched: {domainEvent.GetType().Name} at {domainEvent.OccurredOn}");

            var eventType = domainEvent.GetType();
            var method = typeof(DomainEventDispatcher)
               .GetMethod(nameof(DispatchTyped), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .MakeGenericMethod(eventType);

            method.Invoke(this, new object[] { domainEvent });
        }
    }


    private void DispatchTyped<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        var handlers = _registry.GetHandlers<TEvent>();
        foreach (var handler in handlers)
        {
            handler.Handle(domainEvent);
        }
    }
}
