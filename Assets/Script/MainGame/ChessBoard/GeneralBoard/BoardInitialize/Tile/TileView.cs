using UnityEngine;
using Zenject;

public class TileView : MonoBehaviour
{
    [SerializeField] private GameObject _hoverIndicator;

    public TilePosition Position { get; private set; }

    [Inject] private BoardSettings _settings;
    [Inject] private ITilePositionCalculator _positionCalculator;
    [Inject] private BoardDimensions _dimensions;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private BoxCollider _collider;

    private Vector3 _bounds;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();

        if (_hoverIndicator != null)
        {
            _hoverIndicator.SetActive(false);
        }
    }

    //由 Factory 調用
    [Inject]
    public void Construct(TilePosition position, Vector3 localPosition)
    {
        Position = position;
        transform.localPosition = localPosition;

        gameObject.name = $"Tile_{position.X}_{position.Y}";
        gameObject.layer = LayerMask.NameToLayer("Tiles");
    }

    private void Start()
    {
        GenerateMesh(_bounds);
        gameObject.layer = LayerMask.NameToLayer("Tiles");
        SyncIndicatorSize();
    }

    private void GenerateMesh(Vector3 bounds)
    {
        Mesh mesh = new Mesh();

       // 應用高度到頂點
        Vector3[] vertices = _positionCalculator.CalculateTileVertices(Position.X, Position.Y);

        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;
        _meshRenderer.material = _settings.TileMaterial;


        Bounds meshBounds = mesh.bounds;

        _collider.size = new Vector3(
            _dimensions.TileSize,
            meshBounds.size.y + 1.0f,  // 使用實際高度 + 緩衝值
            _dimensions.TileSize
        );

        _collider.center = meshBounds.center;// center 相對於 TileView 的本地原點
    }


    private void SyncIndicatorSize()
    {
        if (_hoverIndicator == null) return;

        // 取得設定中的格子大小
        float size = _dimensions.TileSize;

        // 根據你使用的 3D 物件類型，調整 Scale：

        // 【情況 A】如果你用的是 Quad (平躺的，Rotation X=90)：
        // Quad 的原始大小是 1x1，所以 X 和 Y 放大 size 倍，Z 維持 1
        //_hoverIndicator.transform.localScale = new Vector3(size, size, 1f);

        // 【情況 B】如果你用的是 3D Cube，或是自己做的平面模型 (沒有旋轉，平躺在 XZ 面)：
        // _hoverIndicator.transform.localScale = new Vector3(size, 0.01f, size);

        // 【情況 C】如果你用的是 Unity 的 Plane：
        // Plane 的預設大小是 10x10，所以要除以 10
         _hoverIndicator.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
    }


    public void SetHoverState(bool isHovering)
    {
        if (_hoverIndicator != null && _hoverIndicator.activeSelf != isHovering)
        {
            _hoverIndicator.SetActive(isHovering);
        }
    }


    public class Factory : PlaceholderFactory<TilePosition, Vector3, TileView>{}
}
