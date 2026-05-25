using Systems.SceneManagement;
using UnityEngine;
using Zenject;
using static SceneDatabase;

// 職責：只負責「狀態轉換 → 場景操作」的對應
// 不知道 Gameplay，不持有遊戲狀態，不訂閱 Signal
public class GameSessionSceneRouter
{
    [Inject] private SignalBus _signalBus;

    public void Route(GameSessionState from, GameSessionState to, BattleType battleType = BattleType.Normal)
    {
        if (SceneController.Instance == null) return;
        Debug.Log($"[SceneRouter] 準備載入場景對應狀態: {to}");

        switch (to)
        {
            case GameSessionState.MainMenu:
                SceneController.Instance.NewTransition()
                    .Unload(Slot.Session)
                    .Unload(Slot.SessionContent)
                    .Load(Slot.Menu, SceneName.MainMenu)
                    .WithOverlay().CleanUnusedAssets().Perform();
                break;

            case GameSessionState.Story:
                if (from == GameSessionState.MainGame)
                    SceneController.Instance.NewTransition()
                        .Unload(Slot.Menu)
                        .HideMainScene(SceneName.MainGameField)
                        .Load(Slot.SessionContent, SceneName.VisualNovel)
                        .WithOverlay().CleanUnusedAssets().Perform();
                else
                    SceneController.Instance.NewTransition()
                        .Unload(Slot.Menu)
                        .Load(Slot.SessionContent, SceneName.VisualNovel)
                        .WithOverlay().CleanUnusedAssets().Perform();
                break;

            case GameSessionState.MainGame:
                bool isMainGameLoaded = SceneController.Instance.sceneSlots.ContainsValue(SceneName.MainGameField);
                bool isNewGameStart = (from == GameSessionState.MainMenu) ||
                                      (from == GameSessionState.Story && !isMainGameLoaded);
                if (isNewGameStart)
                    SceneController.Instance.NewTransition()
                        .Unload(Slot.Menu)
                        .Unload(Slot.SessionContent)
                        .Load(Slot.Session, SceneName.MainGameField, setActive: true)
                        .WithOverlay().CleanUnusedAssets().Perform();
                else
                    SceneController.Instance.NewTransition()
                        .Unload(Slot.SessionContent)
                        .ShowMainScene(SceneName.MainGameField)
                        .WithOverlay()
                        .OnComplete(() => {
                            _signalBus.Fire(new SceneReactivatedSignal(SceneName.MainGameField));
                        })
                        .Perform();
                break;

            case GameSessionState.Battle:
                string targetBattleScene = ResolveBattleScene(battleType);
                SceneController.Instance.NewTransition()
                    .HideMainScene(SceneName.MainGameField)
                    .Load(Slot.SessionContent, ResolveBattleScene(battleType))
                    .WithOverlay()
                    .OnComplete(() => {
                        _signalBus.Fire(new SceneReactivatedSignal(targetBattleScene));
                    })
                    .Perform();
                break;

            case GameSessionState.Shop:
                SceneController.Instance.NewTransition()
                    .HideMainScene(SceneName.MainGameField)
                    .Load(Slot.SessionContent, SceneName.Shop)
                    .WithOverlay()
                    .OnComplete(() => {
                        _signalBus.Fire(new SceneReactivatedSignal(SceneName.Shop));
                    })
                    .Perform();
                break;

            case GameSessionState.Ending:
                SceneController.Instance.NewTransition()
                    .Unload(Slot.Session)
                    .Unload(Slot.SessionContent)
                    .Load(Slot.Session, SceneName.Credits)
                    .WithOverlay().CleanUnusedAssets().Perform();
                break;
        }
    }

    private string ResolveBattleScene(BattleType type) => type switch
    {
        BattleType.Chase => SceneName.ChaseBattle,
        BattleType.Escape => SceneName.EscapeBattle,
        BattleType.Loot => SceneName.LootBattle,
        BattleType.Event => SceneName.EventBattle,
        BattleType.Demonstration => SceneName.DemonstrationBattle,
        BattleType.Final => SceneName.FinalBattle,
        _ => SceneName.NormalBattle
    };
}
