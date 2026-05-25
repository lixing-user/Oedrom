using System;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/WaitAnyPlayerMove")]
public class WaitAnyPlayerMoveCondition : MicroConditionSO
{
    // [NonSerialized] 非常重要，防止 ScriptableObject 的狀態在退出 Play Mode 後殘留
    [NonSerialized] private bool _hasPlayerMoved;

    public override void OnStart(SceneGameActionContext context)
    {
        _hasPlayerMoved = false;

        // 透過 Context 取得 SignalBus (你需要確保 GameActionContext 裡有 SignalBus 的參考)
        if (context.SignalBus != null)
        {
            context.SignalBus.Subscribe<PieceMovedSignal>(OnPieceMoved);
        }
        else
        {
            Debug.LogError("[Tutorial] WaitAnyPlayerMoveCondition 找不到 SignalBus!");
        }
    }

    private void OnPieceMoved(PieceMovedSignal signal)
    {
        // 關鍵過濾：我們只在乎「玩家」的移動。
        // 如果不加這個判斷，AI 回合的移動可能也會觸發這個條件！
        if (signal.Team == Team.White) 
        {
            _hasPlayerMoved = true;
            Debug.Log("[Tutorial] 偵測到玩家任意移動，條件達成！");
        }
    }

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        return _hasPlayerMoved;
    }

    public override void OnExit(SceneGameActionContext context)
    {
        // 步驟結束時務必退訂，避免 Memory Leak 或影響到下一個教學步驟
        if (context.SignalBus != null)
        {
            context.SignalBus.Unsubscribe<PieceMovedSignal>(OnPieceMoved);
        }

        _hasPlayerMoved = false;
    }
}