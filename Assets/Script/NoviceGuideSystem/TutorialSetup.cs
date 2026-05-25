using UnityEngine;

#region MainGameField Tutorial
public static class TutorialSetup
{
    // FEN: 4k2r/2r2b1p/p2p3n/4n3/4pP2/1p4p1/1P2P3/8
    public static readonly PieceSetupData[] TutorialPieces =
    {
        // Rank 8
        new(ChessPieceType.King, 4, 7, Team.Black),
        new(ChessPieceType.Rook, 7, 7, Team.Black),
        
        // Rank 7
        new(ChessPieceType.Bishop, 5, 6, Team.Black),
        new(ChessPieceType.Pawn, 7, 6, Team.Black),
        
        // Rank 6
        new(ChessPieceType.Pawn, 0, 5, Team.Black),
        new(ChessPieceType.Rook, 2, 5, Team.Black),
        new(ChessPieceType.Pawn, 3, 5, Team.Black),
        new(ChessPieceType.Knight, 7, 5, Team.Black),
        
        // Rank 5
        new(ChessPieceType.Knight, 4, 4, Team.Black),
        
        // Rank 4
        new(ChessPieceType.Pawn, 4, 3, Team.Black),
        new(ChessPieceType.Pawn, 5, 3, Team.White),
        
        // Rank 3
        new(ChessPieceType.Pawn, 1, 2, Team.Black),
        new(ChessPieceType.Pawn, 6, 2, Team.Black),
        
        // Rank 2
        new(ChessPieceType.Pawn, 1, 1, Team.White),
        new(ChessPieceType.Pawn, 4, 1, Team.White),

        // Rank 1
        //new(ChessPieceType.King, 0, 0, Team.White),
    };

    public static readonly LandmarkSetupData[] TutorialLandmarks =
    {
        new(LandmarkType.Town, 2, 6),
        new(LandmarkType.Town, 6, 5),
        new(LandmarkType.Town, 7, 3),
    };
}

public readonly struct PieceSetupData
{
    public readonly ChessPieceType Type;
    public readonly int X;
    public readonly int Y;
    public readonly Team Team;

    public PieceSetupData(ChessPieceType type, int x, int y, Team team)
    {
        Type = type; X = x; Y = y; Team = team;
    }
}

public readonly struct LandmarkSetupData
{
    public readonly LandmarkType Type;
    public readonly int X;
    public readonly int Y;

    public LandmarkSetupData(LandmarkType type, int x, int y)
    {
        Type = type; X = x; Y = y;
    }
}
#endregion

#region ChaseBattle Tutorial 
public static class ChaseBattleSetup
{
    public static readonly PieceSetupData[] TutorialPieces =
    {
        // Rank 8
        new(ChessPieceType.King, 6, 7, Team.Black),
        
        // Rank 7
        new(ChessPieceType.Knight, 0, 6, Team.White),
        new(ChessPieceType.Rook, 4, 6, Team.White),
        new(ChessPieceType.Rook, 5, 6, Team.Black),
        new(ChessPieceType.Pawn, 6, 6, Team.Black),
        new(ChessPieceType.Pawn, 7, 6, Team.Black),
        
        // Rank 6
        new(ChessPieceType.Queen, 1, 5, Team.Black),
        new(ChessPieceType.Pawn, 3, 5, Team.Black),
        
        // Rank 5
        new(ChessPieceType.Pawn, 3, 4, Team.White),
        new(ChessPieceType.Pawn, 4, 4, Team.Black),
        
        // Rank 4
        new(ChessPieceType.Knight, 1, 3, Team.Black),
        new(ChessPieceType.Pawn, 2, 3, Team.Black),
        new(ChessPieceType.Pawn, 4, 3, Team.White),
        new(ChessPieceType.Pawn, 7, 3, Team.White),
        
        // Rank 3

        new(ChessPieceType.Pawn, 6, 2, Team.White),
        new(ChessPieceType.Bishop, 7, 2, Team.White),
        
        // Rank 2
        new(ChessPieceType.Pawn, 5, 1, Team.White),

        // Rank 1
        new(ChessPieceType.King, 6, 0, Team.White),
    };
}
#endregion

#region EscapeBattle Tutorial 
public static class EscapeBattleSetup
{
    public static readonly EscapePointSetupData[] EscapePoint =
    {
        new (1, 7),
    };

    public static readonly PieceSetupData[] TutorialPieces =
    {
        // Rank 8
        //new(ChessPieceType.King, 1, 7, Team.Black),
        new(ChessPieceType.Rook, 3, 7, Team.Black),
        
        // Rank 7
        new(ChessPieceType.Pawn, 2, 6, Team.Black),
        new(ChessPieceType.Pawn, 4, 6, Team.Black),
        
        // Rank 6
        new(ChessPieceType.Pawn, 5, 5, Team.Black),
        new(ChessPieceType.Bishop, 6, 5, Team.Black),

        // Rank 5
        new(ChessPieceType.Pawn, 0, 4, Team.Black),
        new(ChessPieceType.Knight, 3, 4, Team.Black),
        new(ChessPieceType.Pawn, 7, 4, Team.Black),
        
        // Rank 4
        new(ChessPieceType.Pawn, 1, 3, Team.Black),
        new(ChessPieceType.Pawn, 6, 3, Team.Black),
        
        // Rank 3
        new(ChessPieceType.Queen, 7, 2, Team.White), // 未來由玩家放置
        
        // Rank 2
        new(ChessPieceType.Knight, 5, 1, Team.Black),

        // Rank 1
        new(ChessPieceType.Rook, 7, 0, Team.Black),
    };

    public readonly struct EscapePointSetupData
    {
        public readonly int X;
        public readonly int Y;

        public EscapePointSetupData(int x, int y)
        {
            X = x; Y = y;
        }
    }
}
#endregion

#region FinalBattle Tutorial
public static class FinalBattleTutorialSetup
{
    public static readonly PieceSetupData[] TutorialPieces =
    {
        // Rank 8
        new(ChessPieceType.Rook, 0, 7, Team.Black),
        new(ChessPieceType.Knight, 1, 7, Team.Black),
        new(ChessPieceType.Bishop, 2, 7, Team.Black),
        new(ChessPieceType.Bishop, 5, 7, Team.Black),
        new(ChessPieceType.Knight, 6, 7, Team.Black),
        new(ChessPieceType.Rook, 7, 7, Team.Black),
        
        // Rank 7
        new(ChessPieceType.Pawn, 0, 6, Team.Black),
        new(ChessPieceType.Pawn, 1, 6, Team.Black),
        new(ChessPieceType.Pawn, 2, 6, Team.Black),
        new(ChessPieceType.Pawn, 3, 6, Team.Black),
        new(ChessPieceType.Pawn, 4, 6, Team.Black),
        new(ChessPieceType.Pawn, 5, 6, Team.Black),
        new(ChessPieceType.Pawn, 6, 6, Team.Black),
        new(ChessPieceType.Pawn, 7, 6, Team.Black),
    };
}
#endregion

#region DemonstrationBattle Tutorial
public static class DemonstrationSetup
{
    public static readonly PieceSetupData[] TutorialPieces =
    {
        // Rank 6
        new(ChessPieceType.Pawn, 1, 5, Team.Black),
        new(ChessPieceType.Pawn, 2, 5, Team.Black),
        new(ChessPieceType.King, 4, 5, Team.Black),
        
        // Rank 5
        new(ChessPieceType.Pawn, 4, 4, Team.Black),
        
        // Rank 4
        new(ChessPieceType.Pawn, 0, 3, Team.White),
        new(ChessPieceType.Knight, 4, 3, Team.White),
        
        // Rank 3
        new(ChessPieceType.Rook, 3, 2, Team.White),
        new(ChessPieceType.Pawn, 5, 3, Team.Black),

        // Rank 2
        new(ChessPieceType.Pawn, 1, 1, Team.White),
        new(ChessPieceType.King, 4, 1, Team.White),

        // Rank 1
        new(ChessPieceType.Bishop, 3, 0, Team.White),
    };
}
#endregion