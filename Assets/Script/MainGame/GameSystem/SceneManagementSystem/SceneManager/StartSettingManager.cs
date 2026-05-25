using Systems.SceneManagement;
using UnityEngine;

public class StartSettingManager : MonoBehaviour
{
    public void StartSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slot.Session, SceneDatabase.SceneName.MainGameField)
            .Load(SceneDatabase.Slot.SessionContent, SceneDatabase.SceneName.Cinematic)
            .Unload(SceneDatabase.Slot.Menu)
            .WithOverlay()
            .CleanUnusedAssets()
            .Perform();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
