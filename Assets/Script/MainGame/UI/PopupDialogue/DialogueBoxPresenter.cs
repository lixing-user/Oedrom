using UnityEngine;
using Zenject;

public class DialogueBoxPresenter : MonoBehaviour
{
    [Inject] private IDialogueBoxManager _manager;

    private DialogueBoxView _view;

    #region Initialization and Lifecycle
    void Awake()
    {
        _view = GetComponent<DialogueBoxView>();
    }

    void Start()
    {
        // 輸入事件：View → Presenter → Manager
        _view.OnConfirmRequested += OnConfirmRequested;

        // 狀態事件：Manager → Presenter → View
        _manager.OnLineChanged += OnLineChanged;
        _manager.OnDialogueCompleted += OnDialogueCompleted;

        _view.Hide();
    }

    void OnDestroy()
    {
        _view.OnConfirmRequested -= OnConfirmRequested;

        _manager.OnLineChanged -= OnLineChanged;
        _manager.OnDialogueCompleted -= OnDialogueCompleted;
    }
    #endregion

    #region Event Handlers
    private void OnConfirmRequested() => _manager.Advance();

    private void OnLineChanged(int index)
    {
        var line = _manager.GetCurrentLine();
        _view.Show(line.avatarSprite, line.characterName, line.content, line.anchor);
    }

    private void OnDialogueCompleted() => _view.Hide();
    #endregion
}
