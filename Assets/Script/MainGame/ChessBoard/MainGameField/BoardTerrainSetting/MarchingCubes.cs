using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
    [Inject] private BoardSettings _settings;
    [Inject] private ITerrainDataService _terrainDataService;
    [Inject] private readonly BoardDimensions _dimensions;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    public float[,,] heights;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private Vector3 gridCenter;
    private Vector3 gridBounds;

    #region Properties
    [SerializeField] private MarchingCubeSetting _marchingCubeSetting;
    [SerializeField] private bool visualizeNoise;
    
    private int width;
    private int height;
    private float resolution;
    private float noiseScale;
    private float heightThreshold;
    private bool use3DNoise;

    [Inject]
    private void Init(BoardSettings settings)
    {
        width = _marchingCubeSetting.width;
        height = _marchingCubeSetting.height;
        resolution = _marchingCubeSetting.resolution;
        noiseScale = _marchingCubeSetting.noiseScale;
        heightThreshold = _marchingCubeSetting.heightThreshold;
        use3DNoise = _marchingCubeSetting.use3DNoise;

        heights = _marchingCubeSetting.heights;
    }
    #endregion


    #region Initialize
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Terrain");
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        gridCenter = _settings.BoardCenter;
        gridBounds = _dimensions.GridBounds;

        SetHeights();
        MarchCubes();
        SetMesh();
    }



    private void SetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MarchingCubes Generated Terrain";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
    }

    private void SetHeights()
    {
        heights = new float[width + 1, height + 1, width + 1];
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    if (use3DNoise)
                    {
                        float currentHeight = PerlinNoise3D((float)x / width * noiseScale, (float)y / height * noiseScale, (float)z / width * noiseScale);
                        heights[x, y, z] = currentHeight;
                    }
                    else
                    {
                        float currentHeight = height * Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
                        float distToSufrace;
                        if (y <= currentHeight - 0.5f)
                            distToSufrace = 0f;
                        else if (y > currentHeight + 0.5f)
                            distToSufrace = 1f;
                        else if (y > currentHeight)
                            distToSufrace = y - currentHeight;
                        else
                            distToSufrace = currentHeight - y;
                        heights[x, y, z] = distToSufrace;
                    }
                }
            }
        }
    }

    private void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    float[] cubeCorners = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        cubeCorners[i] = heights[corner.x, corner.y, corner.z];
                    }
                    MarchCube(new Vector3(x, y, z), cubeCorners);
                }
            }
        }
    }
    #endregion


    #region Sub Methods
    private float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);
        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    private int GetConfigIndex(float[] cubeCorners)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > heightThreshold)
                configIndex |= 1 << i;
        }
        return configIndex;
    }

    private void OnDrawGizmosSelected()
    {
        if (!visualizeNoise || !Application.isPlaying)
            return;

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    Gizmos.color = new Color(heights[x, y, z], heights[x, y, z], heights[x, y, z], 1);
                    Vector3 point = new Vector3(x * resolution, y * resolution, z * resolution) - _terrainDataService.gridBounds + _terrainDataService.gridCenter;
                    Gizmos.DrawSphere(point, 0.2f * resolution);
                }
            }
        }
    }
    #endregion


    private void MarchCube(Vector3 position, float[] cubeCorners)
    {
        int configIndex = GetConfigIndex(cubeCorners);
        if (configIndex == 0 || configIndex == 255)
            return;

        int edgeIndex = 0;
        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];
                if (triTableValue == -1)
                    return;

                Vector3 edgeStart = position * resolution + MarchingTable.Edges[triTableValue, 0] * resolution;
                Vector3 edgeEnd = position * resolution + MarchingTable.Edges[triTableValue, 1] * resolution;
                Vector3 vertex = (edgeStart + edgeEnd) / 2;
                
                vertex = vertex - gridBounds + gridCenter;

                vertices.Add(vertex);
                triangles.Add(vertices.Count - 1);
                edgeIndex++;
            }
        }
    }

   
}