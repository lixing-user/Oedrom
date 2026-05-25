using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BattleResultGatekeeper : IInitializable, IDisposable
{
    [Inject] private ITutorialOverrideService _overrideService;
    [Inject] private SignalBus _signalBus;

    // 暫存區：用來保存因 UI 上鎖而無法發送的結算資料
    private Queue<GameEndedSignal> _pendingEndSignals = new Queue<GameEndedSignal>();

    public void Initialize()
    {
        _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
        _signalBus.Subscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);
        _signalBus.Subscribe<ForceBattleResetSignal>(ClearPendingSignals);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<GameEndedSignal>(OnGameEnded);
        _signalBus.Unsubscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);
        _signalBus.Unsubscribe<ForceBattleResetSignal>(ClearPendingSignals);
    }

    private void OnGameEnded(GameEndedSignal signal)
    {
        if (_overrideService.IsFeatureLocked("GameOverUI"))
        {
            // 鎖定中，將信號推入佇列暫存，不向下發送
            _pendingEndSignals.Enqueue(signal);

            if (signal.WinnerTeam == Team.Black)
            {
                Debug.Log("[Gatekeeper] 偵測到失敗，但 UI 上鎖。靜默攔截結算畫面...");
            }
            return;
        }

        ProcessGameEnd(signal);
    }

    private void OnFeatureLockChanged(FeatureLockChangedSignal signal)
    {
        // 判斷是否為 GameOverUI 被「解鎖」
        if (signal.FeatureName == "GameOverUI" && !signal.IsLocked)
        {
            // 檢查是否有暫存在佇列中的結算信號
            if (_pendingEndSignals.Count > 0)
            {
                Debug.Log("[Gatekeeper] 接收到 GameOverUI 解鎖通知，釋放暫存的結算信號！");

                while (_pendingEndSignals.Count > 0)
                {
                    // 取出最先進入佇列的信號
                    GameEndedSignal pendingSignal = _pendingEndSignals.Dequeue();
                    ProcessGameEnd(pendingSignal);
                }
            }
        }
    }

    private void ProcessGameEnd(GameEndedSignal signal)
    {
        Debug.Log("[Gatekeeper] 結算正常放行，發送 ShowGameEndUISignal！");
        _signalBus.Fire(new ShowGameEndUISignal(signal.WinnerTeam, signal.Reason));
    }

    public void ClearPendingSignals()
    {
        _pendingEndSignals.Clear();
        Debug.Log("[Gatekeeper] 偵測到時光倒流，已清空作廢的結算信號。");
    }
}

public struct ShowGameEndUISignal
{
    public Team WinnerTeam { get; }
    public string Reason { get; }

    public ShowGameEndUISignal(Team winnerTeam, string reason)
    {
        WinnerTeam = winnerTeam;
        Reason = reason;
    }
}