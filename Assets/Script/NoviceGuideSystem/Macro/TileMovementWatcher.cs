using UnityEngine;

public class TileMovementWatcher : MonoBehaviour
{
    private ProjectGameActionContext _context;
    private Vector2Int _targetPosition;
    private Team _targetTeam;
    private string _flagToSet;

    public void Initialize(ProjectGameActionContext context, Vector2Int target, Team targetTeam, string flagToSet)
    {
        _context = context;
        _targetPosition = target;
        _targetTeam = targetTeam;
        _flagToSet = flagToSet;

        _context.SignalBus.Subscribe<PieceMovedSignal>(OnPieceMoved);
    }

    private void OnPieceMoved(PieceMovedSignal signal)
    {
        Vector2Int toPos = new Vector2Int(signal.ToPosition.X, signal.ToPosition.Y);

        if (toPos == _targetPosition && (_targetTeam == Team.None || signal.Team == _targetTeam))
        {
            Debug.Log($"[MacroWatcher] 玩家抵達目標 {_targetPosition}，開始執行關聯 Actions");

            // 輪詢執行所有配置好的 Action！
            if (!string.IsNullOrEmpty(_flagToSet))
            {
                Debug.Log($"[MacroWatcher] 寫入完成 Flag: {_flagToSet}");
                // 這裡呼叫你現有的 TutorialProgressService 來寫入 Flag 並發射 GameFlagAddedSignal
                _context.TutorialProgressService.MarkStepCompleted(_flagToSet);
            }

            _context.SignalBus.Unsubscribe<PieceMovedSignal>(OnPieceMoved);
            Destroy(gameObject);
        }
    }
}
