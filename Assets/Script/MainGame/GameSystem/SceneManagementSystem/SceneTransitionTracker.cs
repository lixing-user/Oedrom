using System;
using UnityEngine;
using Zenject;

public class SceneTransitionTracker : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;

    // 核心屬性：標記現在是否正在跨場景載入
    public bool IsTransitioning { get; private set; }

    public SceneTransitionTracker(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        // 1. 聽到這兩個信號，代表「開始跨場景切換」，拉起封鎖線
        _signalBus.Subscribe<RequestSessionStateChangeSignal>(() => IsTransitioning = true);
        _signalBus.Subscribe<ChangeBattleTypeSignal>(() => IsTransitioning = true);
        _signalBus.Subscribe<RequestEnterBattleSignal>(() => IsTransitioning = true);
        _signalBus.Subscribe<RequestEnterShopSignal>(() => IsTransitioning = true);
        _signalBus.Subscribe<BattleResolveSignal>(() => IsTransitioning = true);

        // 2. 聽到這兩個信號，代表「新場景已經完全載入並初始化完畢」，解除封鎖！
        _signalBus.Subscribe<SceneUIReadySignal>(OnSceneReady);
        _signalBus.Subscribe<BoardInitializedSignal>(OnSceneReady);
        _signalBus.Subscribe<ShopInitializedSignal>(OnSceneReady);
        _signalBus.Subscribe<SceneReactivatedSignal>(OnSceneReady);

    }

    public void Dispose()
    {
        _signalBus?.TryUnsubscribe<RequestSessionStateChangeSignal>(() => IsTransitioning = true);
        _signalBus?.TryUnsubscribe<ChangeBattleTypeSignal>(() => IsTransitioning = true);
        _signalBus?.TryUnsubscribe<RequestEnterBattleSignal>(() => IsTransitioning = true);
        _signalBus?.TryUnsubscribe<RequestEnterShopSignal>(() => IsTransitioning = true);
        _signalBus?.TryUnsubscribe<BattleResolveSignal>(() => IsTransitioning = true);
        _signalBus?.TryUnsubscribe<SceneUIReadySignal>(OnSceneReady);
        _signalBus?.TryUnsubscribe<BoardInitializedSignal>(OnSceneReady);
        _signalBus?.TryUnsubscribe<ShopInitializedSignal>(OnSceneReady);
        _signalBus?.TryUnsubscribe<SceneReactivatedSignal>(OnSceneReady);

    }


    private void OnSceneReady()
    {
        if (!IsTransitioning) return; // 避免重複觸發

        IsTransitioning = false;
        Debug.Log("[Tracker] 場景切換完全就緒，解除封鎖線，發射接力棒！");

        // 核心：確保自己的狀態變成 false 後，才叫醒後面的系統
        _signalBus.Fire<SceneTransitionCompletedSignal>();
    }


    // 提供給 Action 強制同步拉起封鎖線的方法
    public void LockTransitionSynchronously()
    {
        IsTransitioning = true;
        Debug.Log("[Tracker] 已被強制同步拉起封鎖線！");
    }
}

public struct SceneTransitionCompletedSignal { }
