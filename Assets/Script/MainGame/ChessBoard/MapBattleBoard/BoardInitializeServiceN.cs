using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BoardInitializeServiceN : IBoardInitializationService
{
    [Inject] private readonly SignalBus _signalBus; 
    [Inject] private readonly BoardDimensions _dimensions; // Domain
    [Inject] private readonly BattleContextHolder _battleContext;
    [Inject] private readonly IBoardQueryService _boardQuery;

    [Inject] private readonly IBoardTileGenerator _tileGenerator; // Infrastructure
    [Inject] private readonly IPieceSpawner _pieceSpawner;
    [Inject] private readonly IEnemyDifficultyService _enemyDifficultyService;


    public IEnumerator InitializeBoardAsync()
    {
        // 1. 生成棋盤格子
        _tileGenerator.GenerateAllTiles(
            _dimensions.GridSpacing,
            _dimensions.BoardSizeX,
            _dimensions.BoardSizeY
        );

        yield return RestoreEntitiesAsync();
        

        Debug.Log("棋盤初始化完成");
    }

    public IEnumerator RestoreEntitiesAsync()
    {
        // 2. 生成棋子
        yield return SpawnAllPieces();

        MarkBattleTargets();

        // 3. 發送訊號通知 UI 顯示雙方手牌（如果有的話）
        _signalBus.Fire(new ToggleHandDeckSignal());
    }

    private IEnumerator SpawnAllPieces()
    {
        int boardSize = _dimensions.BoardSizeX;
        var battle = _battleContext.CurrentBattle;

        if (battle != null)
        {
            switch (battle.BattleType)
            {
                case BattleType.Loot:
                    SetupLootBattle(battle);
                    break;
                case BattleType.Event:
                    SetupEventBattle(battle);
                    break;
                default:
                    SetupNormalBattle(battle);
                    break;
            }
        }
        else
        {
            #region 測試用標準棋盤
            //SpawnPiece(ChessPieceType.Rook, 0, 0, Team.White);
            //SpawnPiece(ChessPieceType.Knight, 1, 0, Team.White);
            //SpawnPiece(ChessPieceType.Bishop, 2, 0, Team.White);
            //SpawnPiece(ChessPieceType.Queen, 3, 0, Team.White);
            //SpawnPiece(ChessPieceType.King, 4, 0, Team.White);
            //SpawnPiece(ChessPieceType.Bishop, 5, 0, Team.White);
            //SpawnPiece(ChessPieceType.Pawn, 0, 1, Team.White);

            //SpawnPiece(ChessPieceType.Rook, 0, boardSize - 1, Team.Black);
            //SpawnPiece(ChessPieceType.Knight, 1, boardSize - 1, Team.Black);
            //SpawnPiece(ChessPieceType.Bishop, 2, boardSize - 1, Team.Black);
            //SpawnPiece(ChessPieceType.King, 3, boardSize - 1, Team.Black);
            //SpawnPiece(ChessPieceType.Queen, 4, boardSize - 1, Team.Black);
            //SpawnPiece(ChessPieceType.Bishop, 5, boardSize - 1, Team.Black);
            #endregion
            foreach (var pieceData in DemonstrationSetup.TutorialPieces)
            {
                SpawnPiece(pieceData.Type, pieceData.X, pieceData.Y, pieceData.Team);
            }
        }

        yield return null;
    }

    private void MarkBattleTargets()
    {
        var battle = _battleContext.CurrentBattle;
        if (battle != null)
        {
            // 標記防守方
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = battle.DefenderId,
                IsPrimary = true
            });

            // 標記攻擊方
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = battle.AttackerId,
                IsPrimary = false
            });

            Debug.Log($"[BoardInitialize] 已標記戰鬥雙方目標");
        }
    }

    private void SetupNormalBattle(BattleSessionData battle)
    {
        Debug.Log("[InitializeService] 初始化一般戰鬥...");

        // 判斷陣營，白方永遠在底部 (Y=0)，黑方永遠在頂部 (Y=5)
        int attackerY = battle.AttackerTeam == Team.White ? 0 : 5;
        int defenderY = battle.DefenderTeam == Team.White ? 0 : 5;

        // X軸的相對位置維持不變，Y軸套用陣營計算結果
        SpawnPieceWithId(battle.AttackerId, battle.AttackerType, 3, attackerY, battle.AttackerTeam);
        SpawnPieceWithId(battle.DefenderId, battle.DefenderType, 2, defenderY, battle.DefenderTeam);

        #region 正式棋盤（根據EnemyDifficulty動態生成）
        // 2. 獲取當前動態難度
        var (levelIndex, difficultyData) = _enemyDifficultyService.GetCurrentDifficulty();

        Debug.Log($"[InitializeService] 載入敵人配置，難度層級: {levelIndex}");
        SpawnDynamicEnemies(difficultyData);
        #endregion
    }

    private void SetupLootBattle(BattleSessionData battle)
    {
        Debug.Log("[InitializeService] 初始化搶劫戰鬥...");

        // 1. 為 LootBattle 補齊缺失的 ID
        battle.AttackerId = PieceID.Generate();
        battle.DefenderId = PieceID.Generate();

        // 2. 補齊防守方的資訊
        battle.DefenderType = ChessPieceType.Knight;
        battle.DefenderTeam = Team.Black;

        #region 正式棋盤
        // 3. 統一使用 SpawnPieceWithId 生成，這樣它們就會正式註冊在 GameBoard 字典裡了！
        SpawnPieceWithId(battle.AttackerId, battle.AttackerType, 3, 0, battle.AttackerTeam);
        SpawnPieceWithId(battle.DefenderId, battle.DefenderType, 2, 5, battle.DefenderTeam);

        // 4. 生成固定的黑方陣容
        SpawnPiece(ChessPieceType.Rook, 5, 5, Team.Black);
        SpawnPiece(ChessPieceType.Pawn, 5, 4, Team.Black);
        SpawnPiece(ChessPieceType.Pawn, 4, 5, Team.Black);

        // 5.處理隨機生成 (Defender Knight + 剩下的 3 枚 Pawn)
        SpawnLootRandomPieces();
        #endregion
    }

    private void SetupEventBattle(BattleSessionData battle)
    {
        Debug.Log("[InitializeService] 初始化事件戰鬥...");

        // 1. 生成玩家
        SpawnPieceWithId(battle.AttackerId, battle.AttackerType, 3, 0, battle.AttackerTeam);

        // 2. 生成敵人
        string formationId = battle.TargetItemId[0];
        var (levelIndex, baseDifficulty) = _enemyDifficultyService.GetCurrentDifficulty();
        SpawnDynamicEventEnemies(formationId, baseDifficulty);
        Debug.Log($"[InitializeService] 準備生成事件敵人陣容: {formationId}");

        // 3. 隨機抓一隻剛生成的黑方棋子，封牠為 Defender！ 
        var allEnemies = _boardQuery.GetPiecesByTeam(Team.Black).ToList();
        if (allEnemies.Count > 0)
        {
            // 隨機選一隻當作主要目標
            battle.DefenderId = allEnemies[UnityEngine.Random.Range(0, allEnemies.Count)].Id;
        }
    }


    private void SpawnPieceWithId(PieceID existingId, ChessPieceType type, int x, int y, Team team)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            _pieceSpawner.SpawnPieceWithId(existingId, type, position, team);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成棋子失敗（指定ID）[{type}] at ({x},{y}): {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }

    private void SpawnPiece(ChessPieceType type, int x, int y, Team team)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            _pieceSpawner.SpawnSinglePiece(type, position, team);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成棋子失敗 [{type}] at ({x},{y}): {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }

    private void SpawnDynamicEnemies(EnemyDifficultyData difficulty)
    {
        int extraEnemiesToSpawn = difficulty.maxEnemyCount - 1;
        int spawnCount = 0;

        // 1. 透過 Query Service 取得棋盤上所有的空位
        var allEmptyPos = _boardQuery.GetAllEmptyPositions();

        // 2. 利用 LINQ 篩選出僅在 y=4 或 y=5 的空位，並轉為 List 以便操作
        var validSpawnPositions = allEmptyPos.Where(p => p.Y == 4 || p.Y == 5).ToList();

        for (int i = 0; i < extraEnemiesToSpawn; i++)
        {
            if (validSpawnPositions.Count == 0)
            {
                Debug.LogWarning("指定生成區域（底部兩排）沒有足夠的空位生成更多敵人！");
                break;
            }

            ChessPieceType randomEnemyType = difficulty.enemyPieceTypes[UnityEngine.Random.Range(0, difficulty.enemyPieceTypes.Count)];

            // 3. 從篩選後的合法空位中隨機挑選一個索引
            int randomIndex = UnityEngine.Random.Range(0, validSpawnPositions.Count);
            PiecePosition spawnPos = validSpawnPositions[randomIndex];

            // 4. 於該座標生成棋子
            SpawnPiece(randomEnemyType, spawnPos.X, spawnPos.Y, Team.Black);
            spawnCount++;

            // 5. 關鍵：將已使用的空位從暫存清單中移除
            // 這樣下一輪迴圈就絕對不會抽到同一個座標，完美確保不重疊
            validSpawnPositions.RemoveAt(randomIndex);
        }
    }

    private void SpawnDynamicEventEnemies(string formationId, EnemyDifficultyData baseDifficulty)
    {
        // 建立一個臨時的 difficulty，繼承目前的數量上限
        EnemyDifficultyData eventDifficulty = new EnemyDifficultyData
        {
            maxEnemyCount = baseDifficulty.maxEnemyCount,
            enemyPieceTypes = new List<ChessPieceType>()
        };

        // 根據事件字串，定義專屬的「怪物池」與「難度微調」
        switch (formationId)
        {
            case "Ambush_Bandits_01":
                // 盜賊事件：只會出步兵，但因為是埋伏，數量強制 +1
                eventDifficulty.enemyPieceTypes.Add(ChessPieceType.Pawn);
                eventDifficulty.maxEnemyCount += 1;
                break;

            case "Event_Wolves":
                // 狼群事件：全都是騎士 (代表高機動性)
                eventDifficulty.enemyPieceTypes.Add(ChessPieceType.Knight);
                break;

            case "Elite_Assassin":
                // 精英暗殺者：只出一隻超強的皇后，無視全域數量
                eventDifficulty.enemyPieceTypes.Add(ChessPieceType.Queen);
                eventDifficulty.maxEnemyCount = 2; // (1隻本身 + 1隻額外生成 = 共2隻)
                break;

            default:
                // 如果沒配對到，就用原本的全域怪物池
                eventDifficulty.enemyPieceTypes = baseDifficulty.enemyPieceTypes;
                break;
        }

        // 呼叫你原本寫好的完美動態生成器！
        SpawnDynamicEnemies(eventDifficulty);
    }

    private void SpawnLootRandomPieces()
    {
        var allEmptyPos = _boardQuery.GetAllEmptyPositions()
                    .Where(p => !(p.X == 5 && p.Y == 5) &&
                                !(p.X == 5 && p.Y == 4) &&
                                !(p.X == 4 && p.Y == 5) &&
                                !(p.X == 2 && p.Y == 5)) // 確保不會重疊 Defender 的位置
                    .ToList();

        // 篩選出合法的敵方生成區 (Y=4 或 Y=5)
        var validEnemyPositions = allEmptyPos.Where(p => p.Y == 4 || p.Y == 5).ToList();

        // --- 放置剩下的 3 枚 Pawn ---
        int remainingPawns = 3;
        for (int i = 0; i < remainingPawns; i++)
        {
            if (validEnemyPositions.Count == 0)
            {
                Debug.LogWarning("生成區域沒有足夠的空位生成剩餘的 Pawn！");
                break;
            }

            // 動態分類 Y=4 和 Y=5 的空位
            var emptyY4 = validEnemyPositions.Where(p => p.Y == 4).ToList();
            var emptyY5 = validEnemyPositions.Where(p => p.Y == 5).ToList();

            PiecePosition spawnPos = default;
            int selectedIndexToRemove = -1;

            // 傾向 Y=4：如果 Y=4 還有位子，優先從裡面抽
            if (emptyY4.Count > 0)
            {
                int rIndex = UnityEngine.Random.Range(0, emptyY4.Count);
                spawnPos = emptyY4[rIndex];
            }
            // 退而求其次：如果 Y=4 滿了，才放 Y=5
            else if (emptyY5.Count > 0)
            {
                int rIndex = UnityEngine.Random.Range(0, emptyY5.Count);
                spawnPos = emptyY5[rIndex];
            }

            // 執行生成並將該座標從可用清單中剔除
            SpawnPiece(ChessPieceType.Pawn, spawnPos.X, spawnPos.Y, Team.Black);

            selectedIndexToRemove = validEnemyPositions.FindIndex(p => p.X == spawnPos.X && p.Y == spawnPos.Y);
            if (selectedIndexToRemove != -1)
            {
                validEnemyPositions.RemoveAt(selectedIndexToRemove);
            }
        }
    }
}

public interface IBoardInitializationService
{
    IEnumerator InitializeBoardAsync();
    IEnumerator RestoreEntitiesAsync();
}