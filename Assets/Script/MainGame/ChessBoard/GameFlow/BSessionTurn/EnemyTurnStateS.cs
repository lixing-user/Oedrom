using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class EnemyTurnStateS : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private ITutorialOverrideService _tutorialOverride;
    private IFlowFacade _flowFacade;
    private IAIService _aiService;
    private IMoveExecutionStrategyFactory _strategyFactory;
    private IMoveModeService _moveModeService;

    private SignalBus _signalBus;
    private IGameBoard _gameBoard;
    private IBoardQueryService _boardQuery;

    public EnemyTurnStateS(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[EnemyTurnState] 進入敵方回合");

        // 解析 AI 服務
        _aiService = _container.TryResolve<IAIService>();
        _strategyFactory= _container.TryResolve<IMoveExecutionStrategyFactory>();
        _moveModeService = _container.TryResolve<IMoveModeService>();
        _gameBoard = _container.TryResolve<IGameBoard>();
        _tutorialOverride = _container.TryResolve<ITutorialOverrideService>();
        _boardQuery = _container.TryResolve<IBoardQueryService>();

        _signalBus = machine.GetSignalBus();
        _flowFacade = machine.GetFlowFacade();
        
        _flowFacade.DisableAllInput();

        // 2. 正確切換移動模式為直接戰鬥模式
        _moveModeService.SetMode(MoveExecutionMode.Direct);

        // 3. 解除可能遺留的棋子鎖定狀態 (需注入 IGameBoard 或相關依賴)
        UnlockBattlePieces();

        if (_aiService == null)
        {
            Debug.LogError("[EnemyTurnState] IAIService 未找到！");
            // 無法執行 AI，直接回到玩家回合
            _flowMachine.ChangeState(GameFlow.Resolution);
            return;
        }

        // 啟動非同步 AI 思考邏輯 (Fire and Forget)
        _ = ExecuteAITurnAsync();
    }

    private void UnlockBattlePieces()
    {
        var locked = _gameBoard.GetAllPieces()
            .Where(p => p.IsLocked)
            .ToList();

        foreach (var piece in locked)
            piece.Unlock();

        Debug.Log($"[PlayerTurn] 解鎖 {locked.Count} 個棋子");
    }

    private async Task ExecuteAITurnAsync()
    {
        AIMovement result = null;

        // 1. 攔截點：檢查教學覆寫服務中是否有強制指定的指令
        if (_tutorialOverride != null && _tutorialOverride.TryGetForcedAIMove(out string forcedUci))
        {
            Debug.Log($"[EnemyTurnStateS] 教學覆寫接管，攔截 AI 引擎，強制執行: {forcedUci}");

            // 處理 ForceAINullMoveAction 傳入的 "skip_turn"
            if (forcedUci == "skip_turn")
            {
                Debug.Log("[EnemyTurnStateS] 收到 skip_turn，跳過該回合 AI 動作");
                _flowMachine.ChangeState(GameFlow.Resolution);
                return;
            }
            else
            {
                // 處理 ForceAIMoveAction 傳入的實質移動 (如 "e2e4")
                result = ParseUciToMovement(forcedUci);
                if (result == null || !result.IsSuccess)
                {
                    Debug.LogError($"[EnemyTurnStateS] UCI 教學指令解析失敗或不合法: {forcedUci}，防呆切入結算");
                    _flowMachine.ChangeState(GameFlow.Resolution);
                    return;
                }
            }
        }
        else
        {
            Debug.Log("[EnemyTurnStateS] AI 開始運算...");

            // 1. 等待 AI 引擎給出答案
            result = await _aiService.ExecuteMoveAsync();

            if (!result.IsSuccess)
            {
                Debug.LogError($"[EnemyTurnStateS] AI 行動失敗: {result.ErrorMessage}");
                // 失敗也必須交給結算階段處理，避免狀態機卡死
                _flowMachine.ChangeState(GameFlow.Resolution);
                return;
            }
        }

        // 2. === 直接使用策略執行移動，不發送信號 ===
        // 取得當前的移動模式（例如 Direct 或 Setup）
        var currentMode = _moveModeService.CurrentMode;

        // 從工廠拿取對應的策略
        var strategy = _strategyFactory.GetStrategy(currentMode);

        // 執行移動
        strategy.ExecuteMove(result.PieceId, result.From, result.To);

        Debug.Log($"[EnemyTurnStateS] AI 移動執行完畢: {result.From} -> {result.To}");
        HandleTurnEnd();
    }

    private AIMovement ParseUciToMovement(string uci)
    {
        // 1. 使用 UCIParser 解析字串
        var parseResult = UCIParser.Parse(uci);
        if (!parseResult.IsSuccess)
        {
            Debug.LogError($"[EnemyTurnStateS] UCI 格式解析錯誤: {parseResult.Error}");
            return null;
        }

        var moveData = parseResult.Value;
        var from = new PiecePosition(moveData.FromX, moveData.FromY);
        var to = new PiecePosition(moveData.ToX, moveData.ToY);

        // 2. 透過 BoardQuery 抓取在 from 位置上的棋子實體
        var piece = _boardQuery?.GetPieceAt(from);
        if (piece == null)
        {
            Debug.LogError($"[EnemyTurnStateS] 嘗試強制移動 {from}，但該位置找不到棋子！");
            return null;
        }

        // 3. 返回符合系統格式的 AIMovement
        return AIMovement.Success(piece.Id, from, to);
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
