using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public interface IQuestTrackerView
{
    void SetMainQuestDisplay(bool isVisible, string title = "", string objective = "");
}

public class QuestTrackerView : MonoBehaviour, IQuestTrackerView
{
    [Header("UI 元件引用")]
    [SerializeField] private PanelView mainQuestPanel;              // 主線任務面板
    [SerializeField] private TextMeshProUGUI mainQuestTitleText;       // 主線任務標題
    [SerializeField] private TextMeshProUGUI mainQuestObjectiveText;   // 主線任務當前目標說明



    public void SetMainQuestDisplay(bool isVisible, string title = "", string objective = "")
    {
        mainQuestTitleText.gameObject.SetActive(isVisible);
        mainQuestObjectiveText.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            mainQuestPanel.Show();
            mainQuestTitleText.text = title;
            mainQuestObjectiveText.text = objective;
        }
        else
        {
            mainQuestPanel.Hide();
        }
    }
}
