using System.Collections.Generic;
using UnityEngine;
using static SceneDatabase;

public static class SceneDatabase
{
    public class Slot
    {
        public const string Menu = "Menu";
        public const string Session = "Session";
        public const string SessionContent = "SessionContent";
    }

    public class SceneName
    {
        public const string MainMenu = "Lobby";
        public const string Cinematic = "Cinematic";
        public const string AmbientAudio = "AmbientAudio";
        public const string StartSet = "StartGameSetting";
        public const string VisualNovel = "StoryScene";
        public const string MainGameField = "MainGameField";
        public const string NormalBattle = "NormalBattle";
        public const string LootBattle = "NormalBattle";    //與normalbattle共用場景
        public const string EventBattle = "NormalBattle";
        public const string DemonstrationBattle = "NormalBattle"; 
        public const string FinalBattle = "FinalBattle";
        public const string ChaseBattle = "ChaseBattle";
        public const string EscapeBattle = "EscapeBattle";
        public const string Shop = "Shop";
        public const string Credits = "Credits_Scene";
    }
}
