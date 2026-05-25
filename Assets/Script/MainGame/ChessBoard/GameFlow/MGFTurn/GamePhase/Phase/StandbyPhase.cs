using System.Collections;
using UnityEngine;

// Application / Phase / StandbyPhase.cs
public class StandbyPhase : IGamePhaseState
{
    private GamePhaseStateMachine _machine;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;

    #region 介面實作
    public void OnEnter(GamePhaseStateMachine machine)
    {
        _machine = machine;
        _model = machine.GetModel();
        _facade = machine.GetFacade();

        Debug.Log("[StandbyPhase] === 開始準備階段 ===");

        if (_facade.GetCurrentTeam() == Team.Black)
        {
            Debug.Log("[StandbyPhase] 黑方回合，無補給事件，跳過邏輯...");
            // 直接標記完成並進入下一階段
            _model.IsResourceReplenished = false; // 黑方不補給
            _model.CanProgressToNextPhase = true;
            _machine.ChangePhase(GamePhaseState.MainPhase1);
            return;
        }

        // 重置階段狀態
        _model.IsResourceReplenished = false;
        _model.CanProgressToNextPhase = false;

        // 只負責協調，不包含業務邏輯
        _machine.StartCoroutine(ExecutePhaseCoroutine());
    }

    public void OnUpdate(GamePhaseStateMachine machine){}

    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[StandbyPhase] === 離開準備階段 ===");
        Debug.Log($"  → 資源補給完成: {_model.IsResourceReplenished}");
    }
    #endregion

    #region 核心邏輯
    private IEnumerator ExecutePhaseCoroutine()
    {
        Debug.Log("[StandbyPhase] 開始執行階段流程...");

        // === 步驟 1：補給資源 ===
        yield return ReplenishResourcesAsync();

        // === 步驟 2：抽卡（未來實現）===
        // yield return DrawCardsAsync();

        // === 步驟 3：顯示階段完成提示 ===
        yield return ShowPhaseCompleteUI();

        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        _machine.ChangePhase(GamePhaseState.MainPhase1);
        Debug.Log("[StandbyPhase] === 準備階段結束，進入主要階段1 ===");
    }


    private IEnumerator ReplenishResourcesAsync()
    {
        Debug.Log("[StandbyPhase] [1/3] 補給資源...");

        bool completed = false;
        _facade.ReplenishResources(() => completed = true);

        // 等待完成（含超時保護）
        float timeout = 5f;
        float elapsed = 0f;

        while (!completed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!completed)
        {
            Debug.LogWarning("[StandbyPhase] 資源補給超時！強制繼續...");
            _model.IsResourceReplenished = false;
        }
        else
        {
            _model.IsResourceReplenished = true;
            Debug.Log("[StandbyPhase] 資源補給完成");
        }
    }


    private IEnumerator ShowPhaseCompleteUI()
    {
        Debug.Log("[StandbyPhase] [2/3] 等待 UI 動畫...");

        // TODO: 顯示資源變化的 UI 動畫
        yield return new WaitForSeconds(1.5f);

        Debug.Log("[StandbyPhase] UI 動畫完成");
    }
    #endregion
}
