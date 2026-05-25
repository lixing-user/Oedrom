using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FENAdapter
{
    #region 正向解析（AI用途）
    public string GetFEN(IBoardStateSnapshot boardState)
    {
        var dimensions = boardState.Dimensions;
        string fen = "";

        for (int y = dimensions.BoardSizeY - 1; y >= 0; y--)
        {
            int empty = 0;

            for (int x = 0; x < dimensions.BoardSizeX; x++)
            {
                var position = new PiecePosition(x, y);
                var piece = boardState.GetPieceAt(position);

                if (piece == null)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0)
                    {
                        fen += empty.ToString();
                        empty = 0;
                    }
                    fen += PieceToFENChar(piece);
                }
            }
            if (empty > 0)
                fen += empty.ToString();
            if (y > 0)
                fen += "/";
        }

        // 【修正重點】因為開啟了 pieceDrops = true，Fairy-Stockfish 規定 FEN 必須包含口袋(手牌)資訊。
        // 目前我們先放入空的口袋 []。未來若玩家/AI 有手牌，可以動態把字母塞進括號裡（如 [N]）。
        //fen += "[]";

        // 加上空白做分隔
        fen += boardState.IsWhiteTurn ? " w " : " b ";

        // 補上 Castling 與 En Passant 的預設值 (用 '-' 代表無)
        // 確保這兩個 '-' 前後都有空白分隔
        fen += "- - ";

        fen += $"{boardState.HalfMoveClock} {boardState.FullMoveNumber}";

        return fen;
    }
    private char PieceToFENChar(ChessPiece piece)
    {
        char c = piece.Type switch
        {
            ChessPieceType.Pawn => 'p',
            ChessPieceType.Rook => 'r',
            ChessPieceType.Knight => 'n',
            ChessPieceType.Bishop => 'b',
            ChessPieceType.Queen => 'q',
            ChessPieceType.King => 'k',
            _ => throw new ArgumentException($"未知的棋子類型: {piece.Type}")
        };

        return piece.Team == 0 ? char.ToUpper(c) : c; // 0=白方, 1=黑方     // 白方用大寫，黑方用小寫
    }
    #endregion

    #region 反向解析（載入棋局用途）
    public void LoadFEN(string fen)
    {

    }



    private ChessPieceType CharToPieceType(char c)
    {
        return char.ToLower(c) switch
        {
            'p' => ChessPieceType.Pawn,
            'r' => ChessPieceType.Rook,
            'n' => ChessPieceType.Knight,
            'b' => ChessPieceType.Bishop,
            'q' => ChessPieceType.Queen,
            'k' => ChessPieceType.King,
            _ => throw new ArgumentException($"未知的 FEN 字符: {c}")
        };
    }
    #endregion

}

