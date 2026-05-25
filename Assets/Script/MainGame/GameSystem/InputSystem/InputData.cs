using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InputSource
{
    Board3D,    // 來自棋盤的 Raycast
    CardUI,     // 來自卡牌的 EventSystem
    Keyboard    // 來自鍵盤
}

public struct InputData
{
    public InputSource Source { get; set; }

    #region 通用功能
    public bool IsMouseDown { get; set; }
    public bool IsMouseUp { get; set; }
    public bool IsDragging { get; set; }
    public Vector3 MouseWorldPosition { get; set; }
    public Vector2 ScreenPosition { get; set; }
    public float Timestamp { get; set; }          // 時間戳

    public static readonly InputData Empty = new InputData
    {
        Source = InputSource.CardUI,
        Timestamp = 0f,
        IsMouseDown = false,
        IsMouseUp = false,
        IsDragging = false,
        HasHit = false,
        IsOverUI = false,
        IsOverBoard = false,
    };
    #endregion

    #region 3D 棋盤專用
    public RaycastHit Hit { get; set; }
    public bool HasHit { get; set; }
    public int HitLayer { get; set; }
    public TilePosition HitTilePosition { get; set; }
    public PieceID HitPiece { get; set; }
    #endregion

    #region  UI 卡牌專用
    public PointerEventData EventData { get; set; }
    public CardID HitCard { get; set; }
    public bool IsOverUI { get; set; }            // 是否在 UI 上方
    public bool IsOverBoard { get; set; }         // 是否在棋盤上方
    #endregion

    #region 商店專用
    public string HitShopItemId { get; set; }
    #endregion

    #region 擴充功能
    // === 階段一擴充：完整滑鼠支援 ===
    public bool IsRightMouseDown { get; set; }      // 右鍵（取消/特殊操作）
    public bool IsMiddleMouseDown { get; set; }     // 中鍵（相機控制）
    public Vector2 MouseDelta { get; set; }         // 滑鼠移動量（相機旋轉）
    public float ScrollDelta { get; set; }          // 滾輪（縮放）


    // === 階段二擴充：鍵盤支援 ===
    public bool IsShiftHeld { get; set; }           // 修飾鍵（多選）
    public bool IsCtrlHeld { get; set; }            // 修飾鍵（加選）
    public KeyCode PressedKey { get; set; }         // 按下的鍵（熱鍵）
    public bool IsSpacePressed { get; set; }        // 空白鍵（暫停/加速）
    public bool IsEscapePressed { get; set; }       // Esc鍵（取消/返回）
    #endregion
}