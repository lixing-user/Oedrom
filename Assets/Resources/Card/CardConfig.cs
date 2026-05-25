using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChessPieceCardProduction
{
    [CreateAssetMenu(fileName = "CardConfig", menuName = "ChessPieceCardProduction/ChesspieceData")]
    public class CardConfig : ScriptableObject
    {
        [Header("Base Data")]
        [SerializeField] public ChessPieceType type;
        [SerializeField] public Sprite defaultIcon;

        [Header("Cost")]
        [SerializeField] public int baseCost;

        [Header("Prefabs")]
        [SerializeField] public GameObject prefab3D;
        [SerializeField] public GameObject prefab2D;
        [SerializeField] public GameObject previewPrefab;
    }
}
