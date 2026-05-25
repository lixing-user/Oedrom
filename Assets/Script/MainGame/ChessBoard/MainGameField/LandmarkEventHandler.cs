using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using Zenject;


public interface ILandmarkEventHandler
{
    void HandleLandmark(ChessPiece piece, PiecePosition position);
}


// Application 層：處理地標事件邏輯
public class LandmarkEventHandler : ILandmarkEventHandler
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly ITerrainRepository _terrainRepo;


    public void HandleLandmark(ChessPiece piece, PiecePosition position)
    {
        // 1. 檢查地標類型
        var landmark = _terrainRepo.GetTileAt(position);

        if (landmark.Landmark == LandmarkType.None)
            return;

        // 2. 檢查是否已觸發
        if (landmark.IsEventTriggered)
        {
            Debug.Log($"地標 {landmark.Landmark} at {position} 已觸發過");
            return;
        }

        // 3. 標記為已觸發
        _terrainRepo.MarkEventTriggered(position);

        // 4. 發送信號（讓其他系統響應）
        _signalBus.Fire(new LandmarkTriggeredSignal
        {
            PieceId = piece.Id,
            LandmarkType = landmark.Landmark,
            Position = position,
            PieceType = piece.Type,
            Team = piece.Team
        });

        Debug.Log($"觸發地標: {landmark} at {position}");
    }
}

public class LandmarkTriggeredSignal
{
    public PieceID PieceId { get; set; }
    public LandmarkType LandmarkType { get; set; }
    public PiecePosition Position { get; set; }
    public ChessPieceType PieceType { get; set; }
    public Team Team { get; set; }
}
