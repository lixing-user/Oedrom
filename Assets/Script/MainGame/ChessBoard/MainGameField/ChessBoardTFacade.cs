using UnityEngine;
using Zenject;
using System.Collections;


public enum TerrainType { Plain, Mountain, Forest, Water }
public enum LandmarkType { None, Town, Thief, Caravan }

// 棋盤總控介面
public class ChessBoardTFacade : MonoBehaviour
{
    [SerializeField] private MarchingCubes _marchingCubes;

    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardInitializationServiceT _boardInitialize;

    [Inject] private readonly GameFlowStateMachine _gameFlowMachine;
    [Inject] private readonly GamePhaseFacade _phaseFacade;


    private void Start()
    {
        StartCoroutine(InitializeBoard());
    }

    private IEnumerator InitializeBoard()
    {
        Debug.Log("[BoardFacade] 開始初始化棋盤");

        yield return _boardInitialize.InitializeBoardAsync(_marchingCubes);

        Debug.Log("[BoardFacade] 棋盤初始化完成，發送訊號");
        _signalBus.Fire<BoardInitializedSignal>();
    }
}