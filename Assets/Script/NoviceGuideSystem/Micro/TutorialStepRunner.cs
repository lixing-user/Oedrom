using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class TutorialStepRunner : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;

    [Inject] private PendingTutorialContext _pendingContext;

    [Inject] private SceneGameActionContext _microContext;
    private Coroutine _currentRoutine;


    // 紀錄當前正在執行的序列，以便重置時可以重新啟動
    private string _currentSequenceId;
    private TutorialStep[] _currentSteps;

    void Awake()
    {
        _signalBus.Subscribe<PlayTutorialSequenceSignal>(BeginSequence);
        _signalBus.Subscribe<SceneTransitionCompletedSignal>(OnSceneTransitionCompleted);

        _signalBus.Subscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);
    }

    void Start()
    {
        StartCoroutine(DelayedCheckPendingTutorial());
    }
    private IEnumerator DelayedCheckPendingTutorial()
    {
        // 讓出一個 Frame，確保所有腳本的 Awake 和 Start 都執行完畢
        yield return new WaitForEndOfFrame();
        // (如果場景有淡入動畫，這裡甚至可以改成 yield return new WaitForSeconds(0.5f);)

        CheckPendingTutorial();
    }

    void OnDestroy()
    {
        _signalBus?.Unsubscribe<PlayTutorialSequenceSignal>(BeginSequence);
        _signalBus?.Unsubscribe<SceneTransitionCompletedSignal>(OnSceneTransitionCompleted);
        _signalBus?.Unsubscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);
    }


    private void CheckPendingTutorial()
    {
        if (_pendingContext != null && _pendingContext.HasPending)
        {
            string seqId = _pendingContext.SequenceId;
            TutorialStep[] steps = _pendingContext.Steps;

            // 讀取後立刻清空佈告欄，避免重複執行 (Consume)
            _pendingContext.Consume();

            Debug.Log($"[TutorialStepRunner] 場景載入完畢，接手執行跨場景排程: {seqId}");

            // 確保跨場景的教學也能被回溯機制正確讀取
            _currentSequenceId = seqId;
            _currentSteps = steps;

            // 手動觸發
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(RunSequence(seqId, steps));
        }
    }

    private void BeginSequence(PlayTutorialSequenceSignal signal)
    {
        if (!this.isActiveAndEnabled) return;

        _currentSequenceId = signal.SequenceId;
        _currentSteps = signal.Steps;

        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _currentRoutine = StartCoroutine(RunSequence(signal.SequenceId, signal.Steps));
    }

    private void OnSceneTransitionCompleted()
    {
        if (!this.isActiveAndEnabled) return;
        // 為了安全起見，同樣延遲一幀檢查，確保其他依賴都初始化完畢
        StartCoroutine(DelayedCheckPendingTutorial());
    }

    private void OnBattleResetCompleted(BattleResetCompletedSignal signal)
    {
        if (!this.isActiveAndEnabled || _currentSteps == null) return;

        Debug.Log("[TutorialStepRunner] 偵測到盤面已重置，重新啟動當前教學序列！");
        // 1. 強制停止舊的 Coroutine，防止舊邏輯幽靈干擾
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
            _currentRoutine = null;
        }

        // 重新從頭執行當前的序列
        _currentRoutine = StartCoroutine(RunSequence(_currentSequenceId, _currentSteps));
    }

    IEnumerator RunSequence(string sequenceId, TutorialStep[] steps)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            TutorialStep currentStep = steps[i];

            // --- 初始化所有 Condition ---
            InitConditions(currentStep._completionConditions);
            InitConditions(currentStep._failureConditions);

            // 1. 進入動作 (例如高亮)
            Debug.Log("[Tutorial] 1. 開始執行 actionsOnStart...");
            ExecuteActions(currentStep._actionsOnStart);
            yield return null;

            // --- 雙向檢查迴圈 ---
            bool isStepCompleted = false;
            bool isStepFailed = false;
            while (!isStepCompleted && !isStepFailed)
            {
                // 2. 條件卡點
                isStepCompleted = AreConditionsMet(currentStep._completionConditions);

                // 只有在有設定失敗條件時才檢查，避免空陣列報錯
                if (currentStep._failureConditions != null && currentStep._failureConditions.Length > 0)
                {
                    isStepFailed = AreConditionsMet(currentStep._failureConditions);
                }

                if (!isStepCompleted && !isStepFailed)
                {
                    yield return null; // 兩者皆未達成，等待下一幀
                }
            }

            // --- 清理所有 Condition ---
            Debug.Log($"[Tutorial] 4. 迴圈被打破了！ 成功條件達成? {AreConditionsMet(currentStep._completionConditions)} | 失敗條件達成? {AreConditionsMet(currentStep._failureConditions)}");
            CleanupConditions(currentStep._completionConditions);
            CleanupConditions(currentStep._failureConditions);

            // --- 處理失敗分支 ---
            if (isStepFailed)
            {
                Debug.Log($"[TutorialStepRunner] 步驟 {currentStep.stepId} 失敗！觸發回溯機制。");
                ExecuteActions(currentStep._actionsOnFailure);

                // 中斷當前的 Coroutine。等 Coordinator 重置完畢後，會透過 OnBattleResetCompleted 重新啟動
                yield break;
            }

            // --- 處理成功分支 ---
            ExecuteActions(currentStep._actionsOnCompletion);
        }

        if (_microContext.Global.TutorialProgressService != null)
        {
            Debug.Log($"[TutorialStepRunner] 微觀教學序列 {sequenceId} 結束，寫入進度！");
            _microContext.Global.TutorialProgressService.MarkStepCompleted(sequenceId);

            // 清理暫存，避免重複觸發
            _currentSequenceId = null;
            _currentSteps = null;
        }
    }

    private void InitConditions(MicroConditionSO[] conditions)
    {
        if (conditions == null) return;

        foreach (var condition in conditions)
        {
            if (condition != null) condition.OnStart(_microContext);
        }
    }
    private void CleanupConditions(MicroConditionSO[] conditions)
    {
        if (conditions == null) return;

        foreach (var condition in conditions)
        {
            if (condition != null) condition.OnExit(_microContext);
        }
    }

    bool AreConditionsMet(MicroConditionSO[] conditions)
    {
        if (conditions == null || conditions.Length == 0) return false;

        foreach (var condition in conditions)
        {
            if (!condition.IsConditionMet(_microContext))
            {
                return false;
            }
        }
        return true;
    }

    void ExecuteActions(MicroActionSO[] actions)
    {
        if (actions == null) return;

        foreach (var action in actions)
        {
            action.Execute(_microContext);
        }
    }
}

