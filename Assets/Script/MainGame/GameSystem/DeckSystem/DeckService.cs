using ChessPieceCardProduction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// Application/Deck/DeckService.cs
public interface IDeckService 
{
    void InitializeStartingHand(Team team);
    HandCard DrawCard(ChessPieceType type, Team team);
    List<HandCard> DrawRandomCards(int count, Team team);
    void ReloadCardViews();
    void Reset();
}


public class DeckService : IDeckService
{
    [Inject] private SignalBus _signalBus;
    [Inject] private readonly CardDataFactory _cardFactory;
    [Inject] private readonly IHandService _handService;
   
    private bool _isHandInitialized = false;


    #region 初始化&更新視圖
    // 獲取初始手牌（1*King、1*Bishop、1*Knight、3*Pawn）
    public void InitializeStartingHand(Team team)
    {
        if (_isHandInitialized)
        {
            Debug.LogWarning("[DeckService] 手牌已初始化，跳過重複初始化");
            return;
        }

        var startingCards = new List<ChessPieceType>
        {
            ChessPieceType.King,
            ChessPieceType.Bishop,
            ChessPieceType.Knight,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn
        };

        foreach (var type in startingCards)
        {
            var card = _cardFactory.CreateCard(type, team);
            _handService.AddCard(card);
        }

        _isHandInitialized = true;
        _signalBus.Fire(new CardsLoadedSignal());
    }

    public void ReloadCardViews()
    {
        var currentCards = _handService.GetHandCards();

        if (currentCards.Count == 0)
        {
            Debug.LogWarning("[DeckService] 沒有手牌資料，可能需要初始化");
            return;
        }

        // 觸發事件讓 HandUIManager 執行 RefreshAllCards()，一鍵清空舊 UI 並生成新 UI
        _signalBus.Fire(new CardsLoadedSignal());

        Debug.Log($"[DeckService] 重新載入 {currentCards.Count} 張卡牌視圖");
    }

    // 重置手牌（只在真正需要重新開始時呼叫）
    public void Reset()
    {
        _handService.ResetHand();
        _isHandInitialized = false;
        Debug.Log("[DeckService] 手牌已重置");
    }
#endregion

    #region 抽牌功能
    public HandCard DrawCard(ChessPieceType type, Team team)
    {
        if (!_handService.CanAddCard())
        {
            Debug.LogWarning("手牌已滿，無法抽牌");
            return null;
        }

        var card = _cardFactory.CreateCard(type, team);
        _handService.AddCard(card);
        return card;
    }

    public List<HandCard> DrawRandomCards(int count, Team team)
    {
        var drawnCards = new List<HandCard>();

        // 獲取所有可用的棋子類型（枚舉轉列表）
        var availableTypes = System.Enum.GetValues(typeof(ChessPieceType))
            .Cast<ChessPieceType>()
            .Where(type => type != ChessPieceType.None && type != ChessPieceType.King)
            .ToList();

        for (int i = 0; i < count && _handService.CanAddCard(); i++)
        {
            // 隨機選擇一個棋子類型
            var randomType = availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];

            // 創建並添加卡牌到手牌
            var card = DrawCard(randomType, team);

            // 記錄成功抽取的卡牌
            if (card != null)
                drawnCards.Add(card);
        }

        return drawnCards;
    }
    #endregion
}

