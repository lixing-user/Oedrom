//using ChessPieceCardProduction;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;


//// Infrastructure/Persistence/GameSaveSystem.cs
//public class GameSaveSystem
//{
//    [System.Serializable]
//    public class SaveData
//    {
//        public List<PieceDTO> pieces = new();
//        public List<CardDTO> handCards = new();
//        public int currentMana;
//        public int turn;
//    }

//    public void SaveGame(GameBoardT board, IEnumerable<HandCard> hand)
//    {
//        var saveData = new SaveData();

//        // 轉換 Domain Entities 為 DTOs
//        saveData.pieces = board.GetAllPieces()
//            .Select(p => new PieceDTO(p))
//            .ToList();

//        saveData.handCards = hand
//            .Select(c => new CardDTO(c))
//            .ToList();

//        // 序列化並存檔
//        string json = JsonUtility.ToJson(saveData, true);
//        File.WriteAllText(GetSavePath(), json);
//    }

//    public void LoadGame(
//        GameBoardT board,
//        IHandService handService,
//        IMovementStrategyFactory strategyFactory)
//    {
//        string json = File.ReadAllText(GetSavePath());
//        var saveData = JsonUtility.FromJson<SaveData>(json);

//        // 恢復棋盤
//        foreach (var dto in saveData.pieces)
//        {
//            var piece = dto.ToDomain(strategyFactory);
//            board.AddPiece(piece);
//        }

//        // 恢復手牌
//        foreach (var dto in saveData.handCards)
//        {
//            var card = dto.ToDomain();
//            handService.AddCard(card);
//        }
//    }

//    private string GetSavePath() =>
//        Path.Combine(Application.persistentDataPath, "savegame.json");
//}
