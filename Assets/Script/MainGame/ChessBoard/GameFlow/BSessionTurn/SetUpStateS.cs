using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// 實作戰鬥開始前的設置、佈陣狀態
public class SetUpStateS : IGameFlowState
{
    private IMoveModeService _moveModeService;
    private IPieceDeploymentService _deploymentService;
    private IDeploymentValidator _validator;
    //
    private ISetupFogService _setupFogService;

    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private  SignalBus _signalBus;
    private IGameBoard _gameBoard;
    private BattleContextHolder _battleContext;

    private IFlowFacade _flowFacade;

    // 儲存原始棋子移動策略，用於恢復
    private Dictionary<PieceID, IMovementStrategy> _originalStrategies
        = new Dictionary<PieceID, IMovementStrategy>();

    private bool _strategySwitched = false;

    #region 生命週期
    public SetUpStateS(
        GameFlowStateMachine flowMachine, 
        DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _deploymentService = _container.Resolve<IPieceDeploymentService>();
        _validator = _container.Resolve<IDeploymentValidator>();
        _moveModeService = _container.Resolve<IMoveModeService>();
        _gameBoard = _container.Resolve<IGameBoard>();
        _battleContext = _container.Resolve<BattleContextHolder>();
        //
        _setupFogService = _container.Resolve<ISetupFogService>();

        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[Setup] 進入佈陣階段");

        _strategySwitched = false;

        _moveModeService.SetMode(MoveExecutionMode.Setup);

        _signalBus.Subscribe<ArrangePieceRequestedSignal>(OnAutoArrangeRequested);
        _signalBus.TryUnsubscribe<CleanUpPieceRequestedSignal>(OnCleanUpRequested);
        _signalBus.Subscribe<CleanUpPieceRequestedSignal>(OnCleanUpRequested);

        _signalBus.Subscribe<DeploymentConfirmedSignal>(OnDeploymentConfirmed);
        _signalBus.Subscribe<BoardInitializedSignal>(OnPiecePlaced);
        _signalBus.Subscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Subscribe<PiecesArrangedSignal>(OnPieceArranged);

        _flowFacade.EnableAllInput();    // 允許所有操作
        //ApplySetupStrategiesToExistingPieces();

        if (_battleContext.CurrentBattle == null)
        {
            Debug.Log("[Setup] 偵測到無戰鬥上下文（展演模式），自動跳過佈陣階段，直接進入玩家回合");
            _flowMachine.ChangeState(GameFlow.PlayerTurn);
        }
        //
        else
        {
            _setupFogService.ApplySetupFog();
        }
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[SetUpState] 離開佈陣階段");

        var board = _container.Resolve<IGameBoard>();

        //
        if (_setupFogService != null)
        {
            _setupFogService.ClearFog();
        }

        // 1. 先解鎖戰鬥棋子，確保後續操作不被阻擋
        UnlockBattlePieces();

        // 2. 再恢復移動策略
        RestorePieceMovement(board);

        // 取消訂閱
        _signalBus.TryUnsubscribe<ArrangePieceRequestedSignal>(OnAutoArrangeRequested);
        _signalBus.TryUnsubscribe<CleanUpPieceRequestedSignal>(OnCleanUpRequested);
        _signalBus.TryUnsubscribe<DeploymentConfirmedSignal>(OnDeploymentConfirmed);
        _signalBus.TryUnsubscribe<BoardInitializedSignal>(OnPiecePlaced);
        _signalBus.TryUnsubscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.TryUnsubscribe<PiecesArrangedSignal>(OnPieceArranged);
    }
    #endregion

    #region 業務邏輯
    private void OnDeploymentConfirmed()
    {
        Debug.Log("[Setup] 玩家確認佈陣");


        // 驗證佈陣合法性
        if (_validator.ValidateDeployment())
        {
            Team startingTeam = _battleContext.CurrentBattle?.AttackerTeam ?? Team.White;

            _signalBus.Fire(new GameStartedSignal(
                startingTeam,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            ));

            GameFlow firstTurnFlow = startingTeam == Team.White ? GameFlow.PlayerTurn : GameFlow.EnemyTurn;
            _flowMachine.ChangeState(firstTurnFlow);

            Debug.Log($"[Setup] 佈陣完成，攻擊方為 {startingTeam}，進入 {firstTurnFlow}");
        }
        else
        {
            Debug.LogWarning("[Setup] 佈陣不合法");
            return;
        }
    }

    private void OnAutoArrangeRequested()
    {
        Debug.Log("[SetUpState] 收到自動佈陣請求");

        var result = _deploymentService.AutoArrangePlayerPieces();
        var currentDeployedCount = _deploymentService.GetDeployedPieceCount();

        if (result.Success)
        {
            Debug.Log($"[SetUpState] 自動佈陣成功：{result.PlacedCount} 個棋子，目前佈陣數量：{currentDeployedCount}");
        }
        else
        {
            Debug.LogWarning($"[SetUpState] 自動佈陣失敗：{result.ErrorMessage}");
        }
    }


    private void OnCleanUpRequested()
    {
        Debug.Log("[SetUpState] 收到清除請求");

        var result = _deploymentService.ClearPlayerPieces();

        _originalStrategies.Clear();
        _strategySwitched = false;

        if (result.Success)
        {
            Debug.Log($"[SetUpState] 清除成功：{result.ClearedCount} 個棋子");
        }
        else
        {
            Debug.LogWarning("[SetUpState] 清除失敗");
        }
    }

    /// <summary>
    /// 當有棋子被放置時，嘗試切換策略（只執行一次）
    /// </summary>
    private void OnPiecePlaced(BoardInitializedSignal signal)
    {
        ApplySetupStrategiesToExistingPieces();
    }

    private void OnCardPlaced(PieceSpawnedSignal signal)
    {
        if (_battleContext.CurrentBattle == null) return;

        var board = _container.Resolve<IGameBoard>();
        var piece = board.GetPieceById(signal.PieceId);

        if (piece != null &&
          piece.Team == Team.White &&
          piece.CurrentPosition != null &&
          piece.CurrentPosition.Y < 2 &&
          !_originalStrategies.ContainsKey(piece.Id))
        {
            var dimensions = _container.Resolve<BoardDimensions>();
            var setupStrategy = new SetupPhaseMoveStrategy(dimensions);

            // 儲存原始策略並切換
            _originalStrategies[piece.Id] = piece.MovementStrategy;
            piece.SetMovementStrategy(setupStrategy);

            Debug.Log($"[Setup] 從卡牌放置棋子 {piece.Id}:已切換移動策略");
        }
    }

    private void OnPieceArranged(PiecesArrangedSignal signal)
    {
        Debug.Log($"[Setup] 從卡牌放置 {signal.PlacedCount} 個棋子已切換移動策略");
        _strategySwitched = false;
        ApplySetupStrategiesToExistingPieces();

    }
    #endregion

    #region 私有方法
    private void ApplySetupStrategiesToExistingPieces()
    {
        if (_battleContext.CurrentBattle == null) return;

        if (_strategySwitched) return;

        var board = _container.Resolve<IGameBoard>();
        var dimensions = _container.Resolve<BoardDimensions>();

        var attackerId = _battleContext.CurrentBattle.AttackerId;
        var defenderId = _battleContext.CurrentBattle.DefenderId;

        var playerPieces = board.GetAllPiecesOnBoard()
            .Where(p => p.Team == Team.White &&
                        p.CurrentPosition?.Y < 2 &&
                        p.Id != attackerId && 
                        p.Id != defenderId)
            .ToList();

        if (playerPieces.Count > 0)
        {
            var setupStrategy = new SetupPhaseMoveStrategy(dimensions);
            foreach (var piece in playerPieces)
            {
                // 如果已經存在就不要重複覆蓋
                if (!_originalStrategies.ContainsKey(piece.Id))
                {
                    _originalStrategies[piece.Id] = piece.MovementStrategy;
                    piece.SetMovementStrategy(setupStrategy);
                }
            }

            Debug.Log($"[Setup] 主動偵測：已切換 {playerPieces.Count} 個棋子的移動策略");
            _strategySwitched = true; // 標記已處理，避免後續收到 BoardInitializedSignal 重複執行
        }

        LockBattlePieces(); // 確保攻擊/防守方被鎖定
    }

    private void RestorePieceMovement(IGameBoard board)
    {
        int totalStrategies = _originalStrategies.Count;
        if (totalStrategies == 0) return;

        int restoredCount = 0;
        int missingCount = 0;

        Debug.Log($"[Restore] 開始恢復，總共有 {totalStrategies} 個策略待處理");

        foreach (var kvp in _originalStrategies)
        {
            var pieceId = kvp.Key;
            var originalStrategy = kvp.Value;
            var piece = board.GetPieceById(kvp.Key);

            Debug.Log($"[Restore] ID={pieceId}: " +
                  $"Strategy={originalStrategy?.GetType().Name}, " +
                  $"PieceFound={piece != null}");

            if (piece != null)
            {
                piece.SetMovementStrategy(kvp.Value);
                restoredCount++;
            }
            else
            {
                missingCount++;
                Debug.LogError($"[Restore] 找不到 PieceID: {pieceId}");
            }
        }

        _originalStrategies.Clear();
        Debug.Log($"[Restore] 完成！總數:{totalStrategies}, 成功:{restoredCount}, 遺失:{missingCount}");
    }

    private void LockBattlePieces()
    {
        var pending = _battleContext.CurrentBattle;
        if (pending == null)
        {
            Debug.Log("[SetupPhase] 無戰鬥上下文，跳過鎖定");
            return;
        }

        Debug.Log($"[LockBattlePieces] AttackerId: {pending.AttackerId}");
        Debug.Log($"[LockBattlePieces] DefenderId: {pending.DefenderId}");
        var attacker = _gameBoard.GetPieceById(pending.AttackerId);
        var defender = _gameBoard.GetPieceById(pending.DefenderId);

        Debug.Log($"[Lock] attacker HashCode: {attacker?.GetHashCode()}");
        Debug.Log($"[Lock] defender HashCode: {defender?.GetHashCode()}");

        attacker?.Lock();
        defender?.Lock();

        Debug.Log($"[SetupPhase] 鎖定棋子 → " +
                  $"Attacker: {pending.AttackerType} / " +
                  $"Defender: {pending.DefenderType}");
    }

    private void UnlockBattlePieces()
    {
        var pending = _battleContext.CurrentBattle;
        if (pending == null) return;

        var attacker = _gameBoard.GetPieceById(pending.AttackerId);
        var defender = _gameBoard.GetPieceById(pending.DefenderId);

        attacker?.Unlock();
        defender?.Unlock();
    }
    #endregion
}


public class DeploymentConfirmedSignal { }
