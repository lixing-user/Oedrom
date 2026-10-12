using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UniversalCheckmateDirector : IInitializable, IDisposable
{
    [Inject] private CoroutineService _coroutineService;
    [Inject] private SignalBus _signalBus;
    [Inject] private BattleContextHolder _battleContext;
    [Inject] private ICameraPresenter _cameraPresenter;
    [Inject] private IPieceViewRegistry _viewRegistry;
    [Inject] private CheckmateUXView _view;

    private Coroutine _visualSequenceCoroutine;

    public void Initialize()
    {
        _signalBus.Subscribe<CheckmateOccurredSignal>(OnCheckmateOccurred);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<CheckmateOccurredSignal>(OnCheckmateOccurred);
    }

    private void OnCheckmateOccurred(CheckmateOccurredSignal signal)
    {
        // 1. 啟動視覺演出協程
        if (_visualSequenceCoroutine != null)
            _coroutineService.StopCoroutine(_visualSequenceCoroutine);

        _visualSequenceCoroutine = _coroutineService.StartCoroutine(PlayGameEndVisualSequence(signal));
    }

    private IEnumerator PlayGameEndVisualSequence(CheckmateOccurredSignal signal)
    {
        var battle = _battleContext.CurrentBattle;
        if (battle == null) yield break;

        Debug.Log($"[GameEndDirector] 啟動遊戲結束視覺演出！ 類型: {battle.BattleType}");

        // --- A. 情境解析與鏡頭對焦 ---
        Transform focusTarget = null;
        bool isEscapeSuccess = (battle.BattleType == BattleType.Escape && signal.WinnerTeam == Team.White);

        if (isEscapeSuccess)
        {
            // 逃脫戰勝利：聚焦在逃脫成功的棋子 (Attacker)
            var escaperView = _viewRegistry.GetView(battle.AttackerId);
            if (escaperView != null) focusTarget = escaperView.transform;
        }
        else
        {
            // 一般戰鬥 / 追擊戰勝利：聚焦在被擊破的棋子
            // 如果白方贏，代表黑方(防守方)死掉；反之亦然
            PieceID defeatedPieceId = (signal.WinnerTeam == Team.White) ? battle.DefenderId : battle.AttackerId;
            var defeatedView = _viewRegistry.GetView(defeatedPieceId);
            if (defeatedView != null) focusTarget = defeatedView.transform;
        }

        // --- B. 執行運鏡與時間緩速 ---
        if (focusTarget != null)
        {
            _cameraPresenter.RequestPieceFocusMode(focusTarget);
        }

        // 營造史詩感的慢動作 (注意：使用 timeScale 改變時，WaitForSeconds 要改用 Realtime)
        Time.timeScale = 0.3f;

        // --- C. 指揮 View 播放 UI 動畫 ---
        if (isEscapeSuccess)
        {
            _view.PlayEscapeEffect();
        }
        else
        {
            _view.PlayCheckmateEffect(signal.WinnerTeam);
        }

        // --- D. 緩衝等待 (給玩家充分享受勝利/失敗的特效) ---
        // 這裡等待 2.5 秒的「真實時間」(不受 timeScale 變慢影響)
        yield return new WaitForSecondsRealtime(2.5f);

        // --- E. 演出結束，復原環境並解鎖 ---
        Time.timeScale = 1f;
        _cameraPresenter.RequestDefaultMode();
        _view.HideAllEffects();
        _visualSequenceCoroutine = null;

        // 4. 通知邏輯層：表演結束，交還控制權
        Debug.Log("[GameEndDirector] 演出完畢，解除 GameOverUI 鎖定");
        _signalBus.Fire(new CheckmateVisualCompletedSignal(signal.WinnerTeam, signal.Reason));
    }
}

public struct CheckmateOccurredSignal
{
    public Team WinnerTeam { get; }
    public string Reason { get; }
    public CheckmateOccurredSignal(Team winnerTeam, string reason)
    {
        WinnerTeam = winnerTeam;
        Reason = reason;
    }
}

public struct CheckmateVisualCompletedSignal
{
    public Team WinnerTeam { get; }
    public string Reason { get; }
    public CheckmateVisualCompletedSignal(Team winner, string reason)
    {
        WinnerTeam = winner;
        Reason = reason;
    }
}