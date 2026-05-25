using UnityEngine;
using System;

public enum DialogueBoxState { Playing, WaitingForAction, Completed }

public class DialogueBoxHandler : IDialogueBoxManager
{
    private ShowDialogueAction _currentAction;
    private int _currentLineIndex;
    private DialogueBoxState _state = DialogueBoxState.Completed;

    public event Action OnDialogueCompleted;
    public event Action OnWaitingForAction;
    public event Action<int> OnLineChanged;    // Presenter 訂閱此事件來知道要刷新 View


    public void Load(ShowDialogueAction action)
    {
        _currentAction = action;
        _currentLineIndex = 0;
        _state = DialogueBoxState.Playing;
        OnLineChanged?.Invoke(_currentLineIndex); // 通知 Presenter，我不直接改 View
    }

    //public void HideDialogue()
    //{
    //    _state = DialogueBoxState.WaitingForAction;

    //}

    public void Advance()
    {
        if (_state != DialogueBoxState.Playing) return;

        _currentLineIndex++;

        if (ShouldBlock())
        {
            _state = DialogueBoxState.WaitingForAction;
            OnWaitingForAction?.Invoke();
            return;
        }

        if (_currentLineIndex < _currentAction.dialogueLines.Length)
            OnLineChanged?.Invoke(_currentLineIndex);
        else
        {
            _state = DialogueBoxState.Completed;
            OnDialogueCompleted?.Invoke();
        }
    }

    public void Resume()
    {
        if (_state != DialogueBoxState.WaitingForAction) return;
        _state = DialogueBoxState.Playing;
        OnLineChanged?.Invoke(_currentLineIndex);
    }


    #region Helper Methods
    public TutorialDialogueLine GetCurrentLine()
    {
        // 取得當前這句話的完整資料
        DialogueLineData currentData = _currentAction.dialogueLines[_currentLineIndex];

        Sprite finalAvatar = null;
        string finalName = "???";

        if (currentData.speaker != null)
        {
            finalName = currentData.speaker.characterName;
            finalAvatar = currentData.speaker.GetAvatar(currentData.emotion); // 透過 Profile 解析表情
        }

        // 將資料包裝成給 View 使用的物件
        return new TutorialDialogueLine(
            finalAvatar,
            finalName,
            currentData.content,
            currentData.anchor
        );
    }

    public DialogueBoxState CurrentState => _state;

    private bool ShouldBlock()
    {
        if (_currentAction.dialogueLines == null || _currentLineIndex >= _currentAction.dialogueLines.Length)
            return false;

        return _currentAction.dialogueLines[_currentLineIndex].pauseAfterThisLine;
    }
    #endregion
}

public interface IDialogueBoxManager
{
    event Action OnDialogueCompleted;
    event Action OnWaitingForAction;
    event Action<int> OnLineChanged;    // Presenter 訂閱此事件來知道要刷新 View

    void Load(ShowDialogueAction action);
    void Advance();
    void Resume();
    TutorialDialogueLine GetCurrentLine();
    DialogueBoxState CurrentState { get; }
}