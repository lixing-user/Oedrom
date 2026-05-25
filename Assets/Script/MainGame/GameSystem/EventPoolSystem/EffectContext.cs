using UnityEngine;
using Zenject;

/// <summary>
/// 效果執行上下文
/// </summary>
public interface IEffectContext
{
    SignalBus SignalBus { get; }
    PlayerResource PlayerResource { get; }
    HandDeck PlayerHand { get; }
    IDeckService DeckService { get; }
    IGameBoard Board { get; }
    IBoardQueryService BoardQuery { get; }
    ICombatModifierService CombatModifierService { get; }
    PieceID TargetPieceId { get; }
}


public class EffectContext : IEffectContext
{
    public SignalBus SignalBus { get; }

    // 1. 資源與狀態
    public PlayerResource PlayerResource { get; }

    // 2. 牌組與卡牌 
    public HandDeck PlayerHand { get; }
    public IDeckService DeckService { get; }

    // 3. 棋盤與棋子查詢/操作
    public IGameBoard Board { get; }
    public IBoardQueryService BoardQuery { get; }

    public ICombatModifierService CombatModifierService { get; }
    // 4. 觸發來源
    public PieceID TargetPieceId { get; }

    public EffectContext(
        PlayerResource playerResource,
        HandDeck playerHand,
        IDeckService deckService,
        IGameBoard board,
        IBoardQueryService boardQuery,
        ICombatModifierService combatModifierService,
        PieceID pieceID,
        SignalBus signalBus)
    {
        PlayerResource = playerResource;
        PlayerHand = playerHand;
        DeckService = deckService;
        Board = board;
        BoardQuery = boardQuery;
        SignalBus = signalBus;
        CombatModifierService = combatModifierService;
        TargetPieceId = pieceID;
    }
}


// Application/EventSystem/EffectContextFactory.cs
public class EffectContextFactory
{
    private readonly PlayerResource _playerResource;
    private readonly HandDeck _playerHand;
    private readonly IDeckService _deckService;
    private readonly IGameBoard _gameBoard;
    private readonly IBoardQueryService _boardQuery;
    private readonly ICombatModifierService _combatModifierService;
    private readonly SignalBus _signalBus;


    public EffectContextFactory(
        PlayerResource playerResource,
        HandDeck playerHand,
        IDeckService deckService,
        IGameBoard gameBoard,
        IBoardQueryService boardQuery,
        ICombatModifierService combatModifierService,
        SignalBus signalBus)
    {
        _playerResource = playerResource;
        _playerHand = playerHand;
        _deckService = deckService;
        _gameBoard = gameBoard;
        _boardQuery = boardQuery;
        _combatModifierService = combatModifierService;
        _signalBus = signalBus;
    }


    public IEffectContext Create(PieceID targetPieceId)
    {
        return new EffectContext(
            _playerResource,
            _playerHand,
            _deckService,
            _gameBoard,
            _boardQuery,
            _combatModifierService,
            targetPieceId,
            _signalBus);
    }
}