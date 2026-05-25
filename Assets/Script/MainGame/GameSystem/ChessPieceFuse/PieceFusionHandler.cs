using ChessPieceCardProduction;
using UnityEngine;
using UnityEngine.UI;

public class PieceFusionHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject fusionPanel;
    public Transform handCardsContent; 
    public Button fuseButton; 
    public Button confirmButton; 
    public Button cancelButton; 
    public GameObject togglePrefab;

    private ToggleGroup toggleGroup; // 用於單選
    private CardConfig selectedCard;


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
