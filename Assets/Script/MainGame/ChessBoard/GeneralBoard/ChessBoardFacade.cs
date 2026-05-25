using UnityEngine;
using System.Collections;
using Zenject;

public class ChessBoardFacade : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardInitializationService _boardInitialize;

    private void Start()
    {
        StartCoroutine(InitializeBoard());
    }

    private IEnumerator InitializeBoard()
    {
        Debug.Log("[BoardFacade] 開始初始化棋盤");

        yield return _boardInitialize.InitializeBoardAsync();

        Debug.Log("[BoardFacade] 棋盤初始化完成，發送訊號");
        _signalBus.Fire<BoardInitializedSignal>();
    }
}
