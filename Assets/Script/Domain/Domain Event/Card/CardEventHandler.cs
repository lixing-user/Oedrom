using UnityEngine;
using Zenject;

public class CardEventHandler :
    IEventHandler<CardAddedEvent>,
    IEventHandler<CardRemovedEvent>,
    IEventHandler<CardStatusChangedEvent>,
    IEventHandler<CardDiscardedEvent>,
    IInitializable
{
    private readonly IEventHandlerRegistry _registry;
    private readonly SignalBus _signalBus;

    public CardEventHandler(IEventHandlerRegistry registry, SignalBus signalBus)
    {
        _registry = registry;
        _signalBus = signalBus;
    }


    public void Initialize()
    {
        // 自動註冊所有實作的事件類型
        _registry.Register<CardAddedEvent>(this);
        _registry.Register<CardRemovedEvent>(this);
        _registry.Register<CardStatusChangedEvent>(this);
    }

    public void Handle(CardAddedEvent @event)
    {
        _signalBus.Fire(new CardAddedSignal { Card = @event.Card });
        _signalBus.Fire(new HandCardsChangedSignal());
        Debug.Log($"[Signal] 手牌新增: {@event.CardType}, 當前手牌數: {@event.CurrentHandSize}");
    }

    public void Handle(CardRemovedEvent @event)
    {
        _signalBus.Fire(new CardRemovedSignal { CardId = @event.CardId });
        _signalBus.Fire(new HandCardsChangedSignal());
        Debug.Log($"[Signal] 卡牌移除: {@event.CardId}, 原因: {@event.Reason}");
    }

    public void Handle(CardDiscardedEvent @event)
    {
        //_signalBus.Fire(new CardDiscardedSignal { CardId = @event.CardId });
        _signalBus.Fire(new HandCardsChangedSignal());
        Debug.Log($"[Signal] 手牌超限丟棄: {@event.CardType}");
    }

    public void Handle(CardStatusChangedEvent @event)
    {
        _signalBus.Fire(new CardStatusChangedSignal
        {
            CardId = @event.CardId,
            NewStatus = @event.NewStatus
        });
    }
}
