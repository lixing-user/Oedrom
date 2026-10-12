using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DialogueHistoryLine
{
    public string character;
    public string content;

    public DialogueHistoryLine(string _character, string _content)
    {
        this.character = _character;
        this.content = _content;
    }
}

public class CharacterSlotState
{
    public string name;
    public CharacterSpriteSize size;
}


// 全域進度 Model（ProjectContext）
public class StoryProgressModel
{
    // === 劇情進度（需要保存） ===
    public Dictionary<string, bool> completedChapters = new Dictionary<string, bool>();
    public Dictionary<string, int> chapterChoices = new Dictionary<string, int>();
    public int currentChapterId = 0;

    // === 業務邏輯方法 ===
    public void MarkChapterComplete(string chapterId)
    {
        completedChapters[chapterId] = true;
    }

    public bool HasCompletedChapter(string chapterId)
    {
        return completedChapters.ContainsKey(chapterId) && completedChapters[chapterId];
    }

    public void ResetAllProgress()
    {
        completedChapters.Clear();
        chapterChoices.Clear();
        currentChapterId = 0;
        Debug.Log("[StoryProgressModel] 劇情進度已完全重置");
    }
}


// 臨時運行時 Model（SceneContext）
public class StoryRuntimeModel
{
    // === 當前劇情狀態 ===
    public string currentChapterId;
    public int currentDialogueIndex = 0;
    public bool isAutoPlay;
    public bool isSkipping; // 快速閱讀
    public bool isUIVisible = true;

    // === 當前顯示資料 ===
    public string currentSpeakerName;
    public string currentDialogueText;
    public Dictionary<string, CharacterSpritePosition> characterPositions = new();
    public Dictionary<string, CharacterSpriteSize> characterSizes = new();

    // === 對話歷史（用於 Log） ===
    public List<DialogueHistoryLine> dialogueHistory = new List<DialogueHistoryLine>();
    public Dictionary<CharacterSpritePosition, CharacterSlotState> stageSlots = new();

    // === 業務邏輯方法 ===
    public void AddDialogueToHistory(string character, string content)
    {
        dialogueHistory.Add(new DialogueHistoryLine(character, content));
    }

    public CharacterSpritePosition? FindCharacter(string name)
    {
        foreach (var kv in stageSlots)
            if (kv.Value.name == name) return kv.Key;
        return null;
    }

    public CharacterSpriteSize GetCharacterSize(string name)
    {
        var pos = FindCharacter(name);
        return pos.HasValue ? stageSlots[pos.Value].size : CharacterSpriteSize.Normal;
    }

    public void PlaceCharacter(string name, CharacterSpritePosition pos, CharacterSpriteSize size)
    {
        var old = FindCharacter(name);
        if (old.HasValue && old.Value != pos) stageSlots.Remove(old.Value);
        stageSlots[pos] = new CharacterSlotState { name = name, size = size }; // 同位置的舊角色等於被取代
    }

    public void RemoveCharacter(string name)
    {
        var pos = FindCharacter(name);
        if (pos.HasValue) stageSlots.Remove(pos.Value);
    }

    public void ClearAllCharacters() => stageSlots.Clear();


    public void ResetForNewChapter()
    {
        currentDialogueIndex = 0;
        currentSpeakerName = "";
        currentDialogueText = "";
        ClearAllCharacters();
        isAutoPlay = false;
        isSkipping = false;
    }
}