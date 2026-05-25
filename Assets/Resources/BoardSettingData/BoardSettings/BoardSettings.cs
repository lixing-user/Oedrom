using System;
using UnityEngine;

[Serializable]
public class BoardSettings
{
    [Header("Board Dimensions")]
    public int BoardSizeX = 8;
    public int BoardSizeY = 8;
    public float TileSize = 1.0f;
    public Vector3 BoardCenter = Vector3.zero;

    [Header("Visual Settings")]
    public float YOffset = 0.25f;
    public float DragOffset = 1.25f;
    public Material TileMaterial;
    public Material[] TeamMaterials;
    public GameObject[] ChessPiecePrefabs;
}


[Serializable]
public class TerrainBoardSettings
{
    [Header("Terrain Specific")]
    public Material[] TerrainMaterials;
    public Material[] LandmarkMaterials;
    public GameObject[] LandmarkPrefabs;
    public float HeightOffset = 0f;
}