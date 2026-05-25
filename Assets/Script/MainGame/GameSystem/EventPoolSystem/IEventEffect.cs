using UnityEngine;
using Zenject;

// Domain/Effects/IEventEffect.cs
/// <summary>
/// 事件效果接口（Domain Layer）
/// </summary>
public interface IEventEffect
{
    string EffectId { get; }
    void Execute(IEffectContext context);
    bool CanExecute(IEffectContext context);
}


