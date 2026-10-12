using DG.Tweening;
using System;
using UnityEngine;
using Zenject;
using System.Collections;

public class PieceVisualPresenter : IInitializable, IDisposable
{
    [Inject] private IPieceViewRegistry _viewRegistry;
    [Inject] private ICameraPresenter _cameraPresenter;

    [Inject] private SignalBus _signalBus;
    [Inject] private CoroutineService _coroutineService;
    [Inject] private BoardContextId _myContextId;

    private Coroutine _currentEnemySequence;

    private PieceID _pendingResetAttacker;
    private PieceID _pendingResetDefender;

    #region Lifecycle
    public void Initialize()
    {
        _signalBus.Subscribe<PieceMovedSignal>(OnPieceMoved);
        _signalBus.Subscribe<PiecesSwappedSignal>(OnPieceSwapped);
        _signalBus.Subscribe<PieceSpawnedSignal>(OnPieceSpawned);
        _signalBus.Subscribe<PieceRemovedSignal>(OnPieceRemoved);
        _signalBus.Subscribe<PieceRestoredSignal>(OnPieceRestored);
        _signalBus.Subscribe<PieceTeamChangedSignal>(OnPieceTeamChanged);

        _signalBus.Subscribe<BattleStartSignal>(OnBattleStart);
        _signalBus.Subscribe<BattleResolveSignal>(OnBattleResolved);
        _signalBus.Subscribe<ForceBattleResetSignal>(OnBattleReset);
        _signalBus.Subscribe<SceneTransitionCompletedSignal>(OnSceneTransitionCompleted);
    }

    public void Dispose()
    {
        _signalBus?.TryUnsubscribe<PieceMovedSignal>(OnPieceMoved);
        _signalBus?.TryUnsubscribe<PieceSpawnedSignal>(OnPieceSpawned);
        _signalBus?.TryUnsubscribe<PieceRemovedSignal>(OnPieceRemoved);
        _signalBus?.TryUnsubscribe<PieceRestoredSignal>(OnPieceRestored);
        _signalBus?.TryUnsubscribe<PieceTeamChangedSignal>(OnPieceTeamChanged);

        _signalBus?.TryUnsubscribe<BattleStartSignal>(OnBattleStart);
        _signalBus?.TryUnsubscribe<BattleResolveSignal>(OnBattleResolved);
        _signalBus?.TryUnsubscribe<ForceBattleResetSignal>(OnBattleReset);
        _signalBus?.TryUnsubscribe<SceneTransitionCompletedSignal>(OnSceneTransitionCompleted);
    }
    #endregion

    #region Signal Handlers
    private void OnPieceSpawned(PieceSpawnedSignal signal)
    {
        if (signal.ContextId != _myContextId) return;

        var view = _viewRegistry.GetView(signal.PieceId);
        Debug.Log($"[PieceViewRegistry] 收到 PieceSpawnedSignal: {signal.PieceId}");

        if (view != null)
        {
            // 讓生成的棋子有一個彈出的動畫效果
            view.transform.localScale = Vector3.zero; 
            Vector3 targetScale = view.GetTargetScale();
            view.transform.DOScale(targetScale, 0.4f).SetEase(Ease.OutBack);
            //view.transform.localScale = Vector3.zero;
            //view.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
        else
        {
            Debug.LogWarning($"[PieceViewRegistry] 找不到 PieceID {signal.PieceId} 對應的 View");
            return;
        }

        view.SyncLogicalPosition();
    }


    private void OnPieceMoved(PieceMovedSignal signal)
    {
        if (signal.ContextId != _myContextId)
            return;

        Debug.Log($"[PieceViewRegistry] 收到 PieceMovedSignal: {signal.PieceId}");
        var view = _viewRegistry.GetView(signal.PieceId);
        if (view == null) return;

        if (view.IsCaptured() || view.gameObject.layer == LayerMask.NameToLayer("Standby"))
        {
            Debug.Log($"[PieceViewRegistry] 棋子 {signal.PieceId} 進入待命區");
            return;
        }

        var boardPos = view.GetPosition();
        if (boardPos == null)
        {
            Debug.LogError($"[PieceViewRegistry] PieceView {signal.PieceId} 的 Position 為 null");
            return;
        }

        Vector3 worldPos = _viewRegistry.GetWorldPosition(boardPos.X, boardPos.Y);

        //view.SetPosition(worldPos);

        float targetYRotation = (view.GetTeam() == Team.White) ? 0f : 180f;
        view.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);

        // 如果是敵方的棋子，就啟動 Camera 追蹤序列
        if (view.GetTeam() == Team.Black)
        {
            // 計算目標世界座標...
            _currentEnemySequence = _coroutineService.StartCoroutine(PlayEnemyMoveSequence(view, worldPos, 1.2f));
        }
        else
        {
            // 如果是玩家自己移動，就不動鏡頭，只播棋子動畫
            view.DOMoveTo(worldPos, 0.8f);
        }

        view.SyncLogicalPosition();
    }


    private void OnPieceRemoved(PieceRemovedSignal signal)
    {
        if (signal.ContextId != _myContextId) return;

        Debug.Log($"[PieceViewRegistry] 銷毀棋子: {signal.PieceId}");
        var view = _viewRegistry.GetView(signal.PieceId);
        if (view == null) return;

        switch (signal.Mode)
        {
            case CaptureMode.Remove:
                view.DODie();
                _viewRegistry.UnregisterView(signal.PieceId);
                Debug.Log($"[PieceViewRegistry] 棋子 {signal.PieceId} 已永久銷毀");
                break;

            case CaptureMode.ToStandby:
                view.SetLayer("Standby");                // 進入待命區：改變 Layer，不銷毀
                Debug.Log($"[PieceViewRegistry] 棋子 {signal.PieceId} Layer 已改為 Standby");
                break;

            default:
                Debug.LogWarning($"[PieceViewRegistry] 未知的 CaptureMode: {signal.Mode}");
                break;
        }

        view.SyncLogicalPosition();
    }


    private void OnPieceRestored(PieceRestoredSignal signal)
    {
        var view = _viewRegistry.GetView(signal.PieceId);
        if (view == null) return;

        // 2. 獲取棋盤該格子的精確世界座標
        Vector3 targetWorldPos = _viewRegistry.GetWorldPosition(signal.Position.X, signal.Position.Y);

        // 3. 恢復為棋子專用的 Layer 
        // (如果在待命區時為了避免點擊衝突改過 Layer，這裡必須改回來，否則後續會點不到)
        view.gameObject.layer = LayerMask.NameToLayer("Pieces");

        // 4. 重置陣營面向 (白方朝前，黑方朝後，確保從待命區拿出來不會東倒西歪)
        float targetYRotation = (signal.Team == Team.White) ? 0f : 180f;
        view.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);

        // 5. 執行短暫的平滑校正動畫，確保它完美貼合在格子正中央
        view.transform.DOMove(targetWorldPos, 0.2f).SetEase(DG.Tweening.Ease.OutQuad);

        view.SyncLogicalPosition();
        Debug.Log($"[PieceVisualPresenter] 棋子 {signal.PieceId} 視覺已精準同步至棋盤 {signal.Position}");
    }


    private void OnPieceTeamChanged(PieceTeamChangedSignal signal)
    {
        var view = _viewRegistry.GetView(signal.PieceId);
        if (view != null)
        {
            view.UpdateTeamVisual(signal.NewTeam);
            Debug.Log($"[PieceViewRegistry] 更新棋子 {signal.PieceId} 視覺為 {signal.NewTeam} 陣營");
        }
    }


    private void OnPieceSwapped(PiecesSwappedSignal signal)
    {
        if (signal.ContextId != _myContextId) return;

        Debug.Log($"[PieceViewRegistry] 收到 PiecesSwappedSignal: {signal.FirstPieceId} <-> {signal.SecondPieceId}");
        var view1 = _viewRegistry.GetView(signal.FirstPieceId);
        var view2 = _viewRegistry.GetView(signal.SecondPieceId);
        if (view1 != null && view2 != null)
        {
            Vector3 tempPos = view1.transform.position;
            view1.SetPosition(view2.transform.position);
            view2.SetPosition(tempPos);
            Debug.Log($"[PieceViewRegistry] 交換位置完成: {signal.FirstPieceId} <-> {signal.SecondPieceId}");
        }
        else
        {
            Debug.LogWarning($"[PieceViewRegistry] 找不到對應的 View 進行交換");
        }
    }


    private void OnBattleReset(ForceBattleResetSignal signal)
    {
        Debug.Log("[PieceVisualPresenter] 收到重置信號，中斷所有正在播放的視覺動畫序列");

        // 1. 停止協程
        if (_currentEnemySequence != null)
        {
            _coroutineService.StopCoroutine(_currentEnemySequence); // 假設你的 CoroutineService 有實作這個方法
            _currentEnemySequence = null;
        }

        // 2. 停止鏡頭追蹤，直接切回 Default
        _cameraPresenter.RequestDefaultMode();
    }

    #region Battle Session
    private void OnBattleStart(BattleStartSignal signal)
    {
        Debug.Log($"[PieceViewRegistry] 戰鬥開始: {signal.AttackerId} vs {signal.DefenderId}");

        // 可以在這裡觸發戰鬥特效、鏡頭動畫等
        var attackerView = _viewRegistry.GetView(signal.AttackerId);
        var defenderView = _viewRegistry.GetView(signal.DefenderId);

        if (attackerView != null && defenderView != null)
        {
            Vector3 battleCenter = defenderView.transform.position;

            // 攻擊方：偏移到左側
            Vector3 attackerOffset = battleCenter + new Vector3(-0.75f, 0, -0.75f);
            attackerView.SetPosition(attackerOffset);

            // 防守方：偏移到右側
            Vector3 defenderOffset = battleCenter + new Vector3(0.75f, 0, 0.75f);
            defenderView.SetPosition(defenderOffset);

            if (attackerView.GetTeam() == Team.Black)
            {
                _coroutineService.StartCoroutine(PlayEnemyBattleChargeSequence(
                    attackerView,
                    attackerOffset,
                    defenderView.transform,
                    0.5f));
            }
            else
            {
                // 讓棋子面向對方
                attackerView.transform.LookAt(defenderView.transform);
                defenderView.transform.LookAt(attackerView.transform);
            }
            
            Debug.Log($"[PieceViewRegistry] 戰鬥站位就緒");

            // 觸發戰鬥動畫
            // attackerView.PlayAttackAnimation();
            // defenderView.PlayDefendAnimation();
        }
    }

    private void OnBattleResolved(BattleResolveSignal signal)
    {
        Debug.Log($"[PieceVisualPresenter] 戰鬥結束，暫存重置名單，等待場景載入...");
        _pendingResetAttacker = signal.AttackerId;
        _pendingResetDefender = signal.DefenderId;
    }

    private void OnSceneTransitionCompleted(SceneTransitionCompletedSignal signal)
    {
        if (_pendingResetAttacker != null && _pendingResetDefender != null)
        {
            Debug.Log($"[PieceVisualPresenter] 戰鬥結束，重置棋子狀態");

            _viewRegistry.ResetPieceToTileCenter(_pendingResetAttacker);
            _viewRegistry.ResetPieceToTileCenter(_pendingResetDefender);

            // 清空暫存，避免重複觸發
            _pendingResetAttacker = null;
            _pendingResetDefender = null;
        }
    }
    #endregion
    #endregion


    private IEnumerator PlayEnemyMoveSequence(PieceView view, Vector3 targetWorldPosition, float moveDuration)
    {
        // --- 步驟 1：鏡頭切換為 TopView 鎖定敵人 ---
        _cameraPresenter.RequestEnemyFocusMode(view.transform);

        // 給鏡頭一點時間飛過去鎖定目標 (時間長度建議與 Cinemachine Brain 的 Default Blend Time 一致)
        float cameraBlendTime = 0.5f;
        yield return new WaitForSeconds(cameraBlendTime);

        // 甦醒後，確認 view 是否還存活。如果不存活，直接中斷協程！
        if (view == null || view.gameObject == null)
        {
            Debug.LogWarning("[PieceVisualPresenter] 棋子已不存在，強制中斷 EnemyMoveSequence");
            yield break;
        }

        // --- 步驟 2：播放棋子移動動畫 ---
        yield return view.DOMoveTo(targetWorldPosition, moveDuration).WaitForCompletion();

        // --- 步驟 3：移動完畢，稍微停留讓玩家看清楚結果 ---
        yield return new WaitForSeconds(0.5f);

        // --- 步驟 4：鏡頭歸位 ---
        _cameraPresenter.RequestDefaultMode();

        _signalBus.Fire<EnemyVisualMoveCompletedSignal>();
    }

    // --- 新增：專為 AI 戰鬥設計的運鏡與解鎖協程 ---
    private IEnumerator PlayEnemyBattleChargeSequence(PieceView attackerView, Vector3 targetWorldPosition, Transform defenderTransform, float moveDuration)
    {
        float cameraBlendTime = 0.8f;

        // 1. 鏡頭切換為 Focus 鎖定攻擊的敵人
        _cameraPresenter.RequestEnemyFocusMode(attackerView.transform);
        yield return new WaitForSeconds(cameraBlendTime); // 給鏡頭飛過去的時間

        if (attackerView == null || attackerView.gameObject == null) yield break;

        // 2. 播放棋子衝刺到戰鬥位置的動畫
        yield return attackerView.transform.DOMove(targetWorldPosition, moveDuration).WaitForCompletion();

        // 3. 轉向面向防守者
        attackerView.transform.LookAt(defenderTransform);
        yield return new WaitForSeconds(0.4f); // 稍微停頓，讓玩家看清楚雙方對峙

        // 4. 鏡頭歸位
        _cameraPresenter.RequestDefaultMode();
        yield return new WaitForSeconds(cameraBlendTime);

        // 5. 發射訊號，讓 MovementPhase 解除死鎖並進入 MainSecPhase！
        _signalBus.Fire<EnemyVisualMoveCompletedSignal>();
    }
}
