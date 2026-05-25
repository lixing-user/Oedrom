using System;
using UnityEngine;

public interface ITurnManager
{
    Team CurrentTurn { get; }
    int TurnNumber {  get; }
    bool IsGameStarted { get; }
    void StartGame();

    event Action<Team> OnTurnChanged; // 場景內部事件
}


// 各個場景棋盤的 TurnManager 總體模板
public class TurnManager : ITurnManager
{
    private Team _currentTurn = Team.White;
    private int _turnNumber;
    private bool _isGameStarted = false;

    public Team CurrentTurn => _currentTurn;
    public int TurnNumber => _turnNumber;
    public bool IsGameStarted => _isGameStarted;
    public event Action<Team> OnTurnChanged;


    // 只提供給狀態機內部調用
    internal void SetTurn(Team newTurn)
    {
        if (_currentTurn == newTurn) return;

        // 如果從黑方轉回白方，代表一個完整的 FullMove 完成了
        if (_currentTurn == Team.Black && newTurn == Team.White)
        {
           // _turnNumber++;
        }

        _currentTurn = newTurn;
        OnTurnChanged?.Invoke(_currentTurn);
        Debug.Log($"Turn switched to {_currentTurn}");
    }

    public void StartGame()
    {
        _isGameStarted = true;
        _currentTurn = Team.White;
        Debug.Log("Game started");
    }
}