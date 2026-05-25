using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/SpawnStandbyPiece")]
public class SpawnStandbyPieceActionSO : MicroActionSO
{
    public ChessPieceType pieceTypeToSpawn = ChessPieceType.Pawn;
    public Team team = Team.White;
    public int count = 2; // 生成數量

    public override void Execute(SceneGameActionContext context)
    {
        if (context.PieceSpawner == null || context.StandbyZone == null)
        {
            Debug.LogError("TutorialAction 缺少 PieceSpawner 或 StandbyZone 參考");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var newPiece = context.PieceSpawner.SpawnPieceToStandbyZone(pieceTypeToSpawn, team);

            // 情況A：如果有抓到 ID，就精準傳入 ID 讓 View 播放華麗生成動畫
            if (newPiece != null)
            {
                context.SignalBus.Fire(new StandbyZoneUpdatedSignal(team, StandbySource.Spawned, newPiece.Id));
            }
            // 情況B：如果因故無法取得 ID，傳入 null，Service 會執行全體平滑排列 (防呆機制)
            else
            {
                context.SignalBus.Fire(new StandbyZoneUpdatedSignal(team, StandbySource.Spawned, null));
            }
        }
    }
}