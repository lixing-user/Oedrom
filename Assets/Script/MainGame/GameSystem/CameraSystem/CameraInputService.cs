using UnityEngine;
using Unity.Cinemachine;
using Zenject;

public class CameraInputService : ITickable
{
    [Inject] private readonly IBoardInputService _boardInputService;
    [Inject] private readonly IBoardInputHandler _boardInputHandler;
    [Inject] private readonly ICardInputManager _cardInputManager;
    [Inject] private readonly IPieceViewRegistry _viewRegistry;
    [Inject] private readonly ICameraPresenter _cameraPresenter;

    private PieceID _lastSelectedPiece;
    private bool _wasCardInputActive = false;

    public void Tick()
    {
        var boardInputData = _boardInputHandler.CollectInputData();
        var cardInputData = _cardInputManager.CollectInputData();

        bool isCardActive = _cardInputManager.HasActiveCardInput;

        // 偵測卡牌操作剛結束的瞬間，將視角切回預設
        if (_wasCardInputActive && !isCardActive)
        {
            _cameraPresenter.RequestDefaultMode();
        }
        _wasCardInputActive = isCardActive;

        // 1. 更新選取狀態 (記錄最後點選的棋子)
        if (boardInputData.IsMouseDown && boardInputData.HitPiece != null)
        {
            _lastSelectedPiece = boardInputData.HitPiece;
        }

        // 2. 處理高優先級的強制中斷 (WASD 打斷)
        HandleFreelookInterrupt();

        // 3. 優先處理卡牌輸入 (拖曳狀態)
        if (_cardInputManager.HasActiveCardInput)
        {
            HandleCardInput(cardInputData);
            return; 
        }

        // 4. 處理一般盤面與鍵盤操作
        HandleBoardInput(boardInputData);
    }

    private void HandleCardInput(InputData inputData)
    {
        if (inputData.IsDragging)
        {
            _cameraPresenter.RequestTopViewMode(); 
        }
        else if (inputData.IsMouseUp)
        {
            _cameraPresenter.RequestDefaultMode();
        }
    }

    private void HandleBoardInput(InputData inputData)
    {
        InputState currentState = _boardInputService.GetCurrentState();

        // --- 視角切換：TopView (T鍵 Toggle && Dragging狀態) ---
        if (currentState == InputState.Dragging || currentState == InputState.Pick)
        {
            _cameraPresenter.RequestTopViewMode();
            return;
        }
        if (currentState == InputState.Idle &&
         _cameraPresenter.CurrentMode == CameraMode.TopView &&
         !Input.GetKey(KeyCode.T)) // 如果不是按著 T 的 Toggle 狀態
        {
            if (inputData.IsMouseUp) _cameraPresenter.RequestDefaultMode();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_cameraPresenter.CurrentMode == CameraMode.TopView)
                _cameraPresenter.RequestDefaultMode();
            else
                _cameraPresenter.RequestTopViewMode();
            return;
        }

        // --- 視角切換：Freelook (空白鍵) ---
        if (inputData.IsSpacePressed)
        {
            _cameraPresenter.RequestFreelookMode();
        }

        // --- 視角切換：Default (Esc鍵返回) ---
        if (inputData.IsEscapePressed)
        {
            _cameraPresenter.RequestDefaultMode();
        }

        // --- 視角切換：Focus (右鍵直接點擊 OR 左鍵選取後按 F 鍵) ---
        bool isFocusTriggeredByRightClick = inputData.IsRightMouseDown && inputData.HitPiece != null;
        bool isFocusTriggeredByFKey = Input.GetKeyDown(KeyCode.F) && _lastSelectedPiece != null;
        if (isFocusTriggeredByRightClick || isFocusTriggeredByFKey)
        {
            PieceID targetPieceId = isFocusTriggeredByRightClick ? inputData.HitPiece : _lastSelectedPiece;

            var pieceView = _viewRegistry.GetView(targetPieceId);    // 從 PieceID 取得 View Transform
            if (pieceView != null)
            {
                _cameraPresenter.RequestPieceFocusMode(pieceView.transform);
            }
        }
    }

    private void HandleFreelookInterrupt()
    {
        if (_cameraPresenter.CurrentMode != CameraMode.Freelook)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                _cameraPresenter.RequestFreelookMode();
            }
        }
    }
}
