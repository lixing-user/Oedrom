using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "OnboardingFlow", menuName = "Flow/Onboarding Flow Script")]
//public class OnboardingFlowSO : ScriptableObject
//{
//    [Header("新手引導總流程表")]
//    public List<OnboardingNodeSO> nodes;
//}
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "QuestSystem/QuestDatabase")]
public class QuestDatabaseSO : ScriptableObject
{
    public QuestSO[] allQuests;
}