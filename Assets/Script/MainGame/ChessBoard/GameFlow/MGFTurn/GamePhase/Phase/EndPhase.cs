using UnityEngine;
using System.Collections;
using Zenject;

public class EndPhase : IGamePhaseState
{
    private GamePhaseStateMachine _machine;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;
    private SignalBus _signalBus;

    #region 介面實作
    public void OnEnter(GamePhaseStateMachine machine)
    {
        _machine = machine;
        _model = machine.GetModel();
        _facade = machine.GetFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[EndPhase] === 進入結束階段 ===");

        _model.CanProgressToNextPhase = false;

        _machine.StartCoroutine(ExecutePhaseCoroutine());
    }
    public void OnUpdate(GamePhaseStateMachine machine)
    {

    }
    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[EndPhase] === 離開結束階段 ===");

        _model.ResetForNewTurn();   // 清理本回合數據

        _facade.DisableAllInput();
    }
    #endregion

    #region 核心邏輯
    private IEnumerator ExecutePhaseCoroutine()
    {
        Debug.Log("[EndPhase] 開始執行結束階段流程...");

        _facade.DisableAllInput();

        // === 步驟 1：回合結束效果處理 ===
        if (_facade.GetCurrentTeam() == Team.White)
        {
            yield return ProcessTurnEndEffects();
        }
        else
        {
            Debug.Log("[EndPhase] 黑方跳過結束效果處理");
        }

        // === 步驟 2：檢查遊戲勝利條件 ===
        //GameEndResult gameEndResult = CheckGameEndConditions();
        //if (gameEndResult.IsGameEnded)
        //{
        //    yield return HandleGameEnd(gameEndResult);
        //    yield break;  // 遊戲結束，不繼續執行
        //}

        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        TriggerTurnEnd();
    }

    private IEnumerator ProcessTurnEndEffects()
    {
        if (_facade.GetCurrentTeam() == Team.Black)
        {
            Debug.Log("[EndPhase] 黑方回合，跳過補給扣除邏輯");
            yield break;
        }

        Debug.Log("[EndPhase] 處理回合結束效果...");

        // 處理持續傷害、回血等效果
        yield return DeductSupplyCostAsync();
        //_facade.ProcessEndOfTurnEffects();

        yield return new WaitForSeconds(0.5f);

        Debug.Log("[EndPhase] 回合結束效果處理完成");
    }

    private IEnumerator DeductSupplyCostAsync()
    {
        Debug.Log("[EndPhase] [1/4] 計算補給消耗...");

        if (_model.MovementRecords.Count == 0)
        {
            Debug.Log("[EndPhase] 本回合無移動，不扣除補給");
            yield break;
        }

        bool completed = false;
        int totalSupplyCost = 0;

        // 計算補給消耗（異步處理）
        _facade.CalculateAndDeductSupply(
            _model.MovementRecords,
            (cost) =>
            {
                totalSupplyCost = cost;
                completed = true;
            }
        );

        float timeout = 5f;        // 等待完成（含超時保護）
        float elapsed = 0f;

        while (!completed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!completed)
        {
            Debug.LogWarning("[EndPhase] 補給扣除超時！強制繼續...");
        }
        else
        {
            Debug.Log($"[EndPhase] 補給扣除完成: -{totalSupplyCost} Supply");
        }
    }

    private void TriggerTurnEnd()
    {
        Debug.Log("[EndPhase] 觸發回合結束事件");

        // 發送回合結束信號（供其他系統使用）
        _signalBus.Fire(new TurnEndedSignal(_facade.GetCurrentTeam()));

        // 延遲觸發 OnAllPhasesCompleted，確保 EndPhase 的 OnExit 先執行
        _machine.StartCoroutine(TriggerOnAllPhasesCompletedDelayed());
    }
    private IEnumerator TriggerOnAllPhasesCompletedDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[EndPhase] 觸發 OnAllPhasesCompleted 事件");

        _machine.TriggerAllPhasesCompleted();
    }
    #endregion
}
