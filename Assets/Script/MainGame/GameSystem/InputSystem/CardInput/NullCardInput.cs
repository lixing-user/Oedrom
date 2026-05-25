using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.EventSystems;

public class NullCardInput : ICardInputManager
{
    public bool IsCardInputEnabled => false;
    public bool HasActiveCardInput => false; // 永遠無卡牌輸入
    public InputData CollectInputData() => InputData.Empty;
    public void SetCardInputEnabled(bool enabled) { }
    public void OnCardPointerDown(CardID cardId, PointerEventData eventData) { }
    public void OnCardDrag(CardID cardId, PointerEventData eventData) { }
    public void OnCardPointerUp(CardID cardId, PointerEventData eventData) { }
    public void ForceReset() { }
}
