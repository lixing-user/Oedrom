using System.Collections;
using UnityEngine;
using Zenject;

public enum ResolutionType
{
    None,
    Battle,
    Shop,
    RandomEvent
}

public class ResolutionPhase : IGamePhaseState
{
    [Inject] private ICameraPresenter _cameraPresenter;
    [Inject] private IMoveHandler _moveHandler;
    [Inject] private GameBoard<TerrainTile> _board;
    [Inject] private readonly CoroutineService _coroutineService;
    [Inject] private readonly ICombatModifierService _combatModifier;

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

        Debug.Log("[ResolutionPhase] === 進入結算階段 ===");
        Debug.Log($"  → 結算類型: {_model.CurrentResolutionType}");

        _model.CanProgressToNextPhase = false;

        _coroutineService.StartCoroutine(ExecutePhaseCoroutine());
    }
    public void OnUpdate(GamePhaseStateMachine machine){}
    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[ResolutionPhase] === 離開結算階段 ===");

        _model.PendingBattle = null;
        _model.PendingBattleResult = null;
        _model.IsBattleTriggered = false;
        _model.CurrentResolutionType = ResolutionType.None;
    }
    #endregion

    #region 核心邏輯
    private IEnumerator ExecutePhaseCoroutine()
    {
        while (!_machine.gameObject.activeInHierarchy)
        {
            yield return null;
        }
        Debug.Log("[ResolutionPhase] 開始處理結果...");

        _cameraPresenter.RequestDefaultMode();
        Debug.Log("[ResolutionPhase] 攝影機已恢復至 Default 模式");

        _facade.DisableAllInput();
        yield return new WaitForSeconds(0.3f);

        if (_model.CurrentResolutionType != ResolutionType.None)
        {
            yield return ProcessResult();
        }

        yield return new WaitForSeconds(0.5f);

        SwitchToNextPhase();
    }

    private void SwitchToNextPhase()
    {
        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        _machine.ChangePhase(GamePhaseState.EndPhase);
        Debug.Log("[ResolutionPhase] === 結算階段結束，進入結束階段 ===");
    }


    #region 各事件流程實作
    private IEnumerator ProcessResult()
    {
        switch (_model.CurrentResolutionType)
        {
            case ResolutionType.Battle:
                yield return ProcessBattleResult();
                break;

            case ResolutionType.Shop:
                yield return ProcessShopResult();
                break;

            case ResolutionType.RandomEvent:
                yield return ProcessRandomEventResult();
                break;

            case ResolutionType.None:
                Debug.Log("[ResolutionPhase] 無需處理結果");
                break;
        }
    }

    private IEnumerator ProcessBattleResult()
    {
        if (_model.PendingBattle == null)
        {
            Debug.LogError("[ResolutionPhase] 缺少 PendingBattle 數據！");
            yield break;
        }

        var result = _model.PendingBattleResult;
        var battleData = _model.PendingBattle;

        bool isEventBattle = battleData.BattleType == BattleType.Event || battleData.BattleType == BattleType.Loot;
        bool isAttackerWinner = result.WinnerTeam == (int)battleData.AttackerTeam;

        if (isAttackerWinner)
        {
            Debug.Log("[ResolutionPhase] 攻擊方勝利！");

            if (isEventBattle)
            {
                // 【事件戰鬥勝利】：原地結算，不需位移，不需吃子
                Debug.Log("[ResolutionPhase] 事件戰鬥勝利，攻擊方留在原地 (恢復主棋盤座標)。");
                _moveHandler.ForceMove(battleData.AttackerId, battleData.AttackerPos);
            }
            else
            {
                // 【普通吃子勝利】：移除防守方，攻擊方佔領該格子
                Debug.Log("[ResolutionPhase] 攻擊方勝利，執行真正移動");

                // 1. 移除防守方
                _moveHandler.ForceCapture(battleData.DefenderPos);

                // 2. 執行攻擊方真正移動
                _moveHandler.ForceMove(battleData.AttackerId, battleData.DefenderPos);
            }
        }
        else  // 防守方勝利或平手
        {
            Debug.Log("[ResolutionPhase] 攻擊方失敗，視覺回退");

            // 移除攻擊方
            _moveHandler.ForceCapture(battleData.AttackerPos);
        }

        Debug.Log($"[ResolutionPhase] 處理戰鬥結果: Winner={result?.WinnerTeam}");

        // === 2. 結算：扣減修改器的「剩餘戰鬥場次」 ===
        // 必須確保每一次有效戰鬥結束都會呼叫這行
        _combatModifier.TickModifiers(battleData);

        // === 3. 清理暫存資料，確保狀態乾淨 ===
        _model.PendingBattle = null;
        _model.PendingBattleResult = null;

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ProcessShopResult()
    {
        yield return null;
    }

    private IEnumerator ProcessRandomEventResult()
    {
        Debug.Log("[ResolutionPhase] 開始執行隨機事件效果...");

        if (_model.PendingRandomEvent != null)
        {
            _facade.ResolveRandomEvent(
             _model.PendingRandomEvent,
             _model.SelectedEventOptionIndex,
             _model.EventTriggerPieceId
         );

            _model.PendingRandomEvent = null;            // 清除已執行的事件
            _model.EventTriggerPieceId = null;
            _model.SelectedEventOptionIndex = -1;
        }
        else
        {
            Debug.Log("[ResolutionPhase] 無 PendingRandomEvent 資料，跳過效果執行");
        }

        Debug.Log("[ResolutionPhase] 隨機事件結算完畢");
        yield return new WaitForSeconds(0.5f);
    }
    #endregion

    #endregion
}
