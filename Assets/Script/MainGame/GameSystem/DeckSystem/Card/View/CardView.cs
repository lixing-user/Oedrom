using ChessPieceCardProduction;
using Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


// Presentation/Hand/CardView.cs (純 UI 組件，無注入)
public class CardView : MonoBehaviour
{
    [Inject] private GameModeStateMachine _gameModeStateMachine;
    //[Inject] private RuntimeCardConfig _runtimeCard;

    public CardID PieceId { get; private set; }

    private GameObject brokenOverlay;
    private Renderer _cardRenderer;
    private Material _instanceMaterial;

    public CardConfig chessPieceData;
    public ChessPieceType[] types;

    public Image[] typeImage;


    private void Start()
    {
        _cardRenderer = GetComponent<Renderer>();
        if (_cardRenderer != null)
        {
            _instanceMaterial = _cardRenderer.material;
        }
    }

    private void OnDestroy()
    {
        if (_cardRenderer != null &&
            _cardRenderer.material != _cardRenderer.sharedMaterial)
        {
            Destroy(_cardRenderer.material);
        }
    }



    public void Initialize(HandCard handCard)
    {
        UpdateCardDisplay(handCard);
    }

    public void UpdateCardDisplay(HandCard handCard)
    {
        //if (_runtimeCard == null) return;
        //Color statusColor = _runtimeCard._status == CardStatus.broken
        //    ? Color.yellow
        //    : Color.white;
        //_cardRenderer.material.color = statusColor;
    }

    private void ApplyStatusVisuals(CardStatus status)
    {
        Color statusColor = (status == CardStatus.broken)
            ? Color.yellow
            : Color.white;

        if (_instanceMaterial != null)
            _instanceMaterial.color = statusColor;

        if (brokenOverlay != null)
            brokenOverlay.SetActive(status == CardStatus.broken);
    }
}