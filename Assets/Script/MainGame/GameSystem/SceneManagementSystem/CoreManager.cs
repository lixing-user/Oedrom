using Systems.SceneManagement;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    void Start()
    {
        //Core Setup for game
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slot.Menu, SceneDatabase.SceneName.MainMenu)
            //.Load(SceneDatabase.Slot.SessionContent, SceneDatabase.SceneName.AmbientAudio)
            .WithOverlay()
            .Perform();
    }

}
