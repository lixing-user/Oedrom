using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class SettingButtonView : MonoBehaviour
{
    [Inject] private OptionPanelHandler optionPanel;

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        BindButton();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindButton();
    }
    void OnSceneUnloaded(Scene scene)
    {
        UnbindButton();
    }


    private void BindButton()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(optionPanel.OpenOptionPanel);
    }

    private void UnbindButton()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.RemoveListener(optionPanel.OpenOptionPanel);
    }
}
