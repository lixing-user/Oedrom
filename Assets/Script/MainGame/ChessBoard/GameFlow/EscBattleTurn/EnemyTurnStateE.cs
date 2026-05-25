using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class EnemyTurnStateE : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private IFlowFacade _flowFacade;
    private IAIService _aiService;
    private IMoveExecutionStrategyFactory _strategyFactory;
    private IMoveModeService _moveModeService;
    private SignalBus _signalBus;

    public EnemyTurnStateE(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[EnemyTurnState] 進入敵方回合");

        // 解析 AI 服務
        _aiService = _container.TryResolve<IAIService>();
        _strategyFactory = _container.TryResolve<IMoveExecutionStrategyFactory>();
        _moveModeService = _container.TryResolve<IMoveModeService>();
        _signalBus = machine.GetSignalBus();

        if (_aiService == null)
        {
            Debug.LogError("[EnemyTurnState] IAIService 未找到！");
            // 無法執行 AI，直接回到玩家回合
            _flowMachine.ChangeState(GameFlow.Resolution);
            return;
        }

        // 啟動非同步 AI 思考邏輯 (Fire and Forget)
        _ = ExecuteAITurnAsync();
        //HandleTurnEnd();
    }

    private async Task ExecuteAITurnAsync()
    {
        Debug.Log("[EnemyTurnStateS] AI 開始運算...");

        // 1. 等待 AI 引擎給出答案
        AIMovement result = await _aiService.ExecuteMoveAsync();

        if (!result.IsSuccess)
        {
            Debug.LogError($"[EnemyTurnStateS] AI 行動失敗: {result.ErrorMessage}");
            // 失敗也必須交給結算階段處理，避免狀態機卡死
            _flowMachine.ChangeState(GameFlow.Resolution);
            return;
        }

        // 2. === 直接使用策略執行移動，不發送信號 ===
        // 取得當前的移動模式（例如 Direct 或 Setup）
        var currentMode = _moveModeService.CurrentMode;

        // 從工廠拿取對應的策略
        var strategy = _strategyFactory.GetStrategy(currentMode);

        // 執行移動！(這裡會直接調用 DirectMoveStrategy 或 SetupMoveStrategy)
        // 假設你的 PieceID 是一個 struct/class，需要將 string 轉換一下，或直接傳遞
        strategy.ExecuteMove(result.PieceId, result.From, result.To);

        Debug.Log($"[EnemyTurnStateS] AI 移動執行完畢: {result.From} -> {result.To}");
        HandleTurnEnd();
    }

    private void HandleTurnEnd()
    {
        // 玩家回合所有階段完成，切換到敵方回合
        Debug.Log("[EnemyTurnStateS] AI 動作完成，進入結算與路由階段");
        _flowMachine.ChangeState(GameFlow.Resolution);
    }


    public void OnExit(GameFlowStateMachine machine)
    {
        _aiService = null;
    }
}
