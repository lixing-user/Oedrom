using ChessPieceCardProduction;
using DG.Tweening;
using UnityEngine;
using Zenject;

//Interfaces Layer
public interface ICardVisualizerService
{
    void CreateDragInstance(ChessPieceType type, GameObject prefab, Vector3 worldPosition, Team team);
    void UpdateDragPosition(Vector3 targetPosition);
    void DestroyDragInstance();

    void ShowBrokenCardWarning();
    void PlayPlacementSuccessEffect();
    void PlayPlacementFailedEffect();
}

public class CardMoveVisualizer : MonoBehaviour, ICardVisualizerService
{
    [Inject] private DiContainer _container;
    [Inject] private BoardSettings _settings;
    [Inject] private ITerrainHeightSampler _heightSampler;

    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float previewScale = 1.0f;
    [SerializeField] private Camera mainCamera;

    public GameObject _worldDragInstance;  // 世界空間拖曳預覽物件
    private Vector3 _targetPosition;
    private bool _isUpdating;


    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (_isUpdating && _worldDragInstance != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            UpdateDragPosition(mouseWorldPos);

            _worldDragInstance.transform.position = Vector3.Lerp(
                _worldDragInstance.transform.position,
                _targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // 備用：固定距離
        return mainCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
    }

    #region 視覺邏輯（prefab）
    public void CreateDragInstance(
        ChessPieceType type,
        GameObject prefab,
        Vector3 worldPosition,
        Team team)
    {
        if (_worldDragInstance != null)
        {
            DestroyDragInstance();
        }

        _worldDragInstance = _container.InstantiatePrefab(
            prefab, 
            worldPosition, 
            Quaternion.identity, 
            null);

        _worldDragInstance.transform.localScale = Vector3.one * previewScale;

        SetupDragInstanceVisuals(type, team);

        _targetPosition = worldPosition;
        _isUpdating = true;
    }

    private void SetupDragInstanceVisuals(ChessPieceType type, Team team)
    {
        var renderer = _worldDragInstance.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = _settings.TeamMaterials[(int)team];
        }

        SetTransparency(0.6f);

        SetLayerRecursive(_worldDragInstance, LayerMask.NameToLayer("Ignore Raycast"));

        var colliders = _worldDragInstance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        _worldDragInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }

    public void UpdateDragPosition(Vector3 mouseWorldPosition)
    {
        float terrainHeight = _heightSampler.GetTerrainHeightAt(mouseWorldPosition);

        _targetPosition = new Vector3(
           mouseWorldPosition.x,
           terrainHeight + hoverHeight,  // 保持固定懸浮高度
           mouseWorldPosition.z
       );

        Debug.DrawLine(
            mouseWorldPosition + Vector3.up * 100f,  
            _targetPosition,                          
            Color.green,
            0.1f
        );
        //_targetPosition = mouseWorldPosition;
    }

    public void DestroyDragInstance()
    {
        if (_worldDragInstance != null)
        {
            Object.Destroy(_worldDragInstance);
            _worldDragInstance = null;
        }
        _isUpdating = false;
    }
    #endregion

    #region 視覺邏輯（材質、動畫）
    private void SetTransparency(float alpha)
    {
        var renderers = _worldDragInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                // 設定材質模式為 Transparent (與原邏輯相同)
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;

                // 使用 DOTween 進行漸變，更平滑
                material.DOFade(alpha, 0.2f).SetLink(_worldDragInstance);
            }
        }
    }

    public void ShowBrokenCardWarning()
    {
        Debug.LogWarning("此卡牌已破損，無法使用");
        // 可選：顯示 UI 提示
        // var warning = Instantiate(_brokenCardWarningPrefab);
        // Destroy(warning, 2f);
    }

    // 放置成功特效
    public void PlayPlacementSuccessEffect()
    {
        Debug.Log("卡牌放置成功");
        // 可選：播放粒子特效、音效
    }

    // 放置失敗特效
    public void PlayPlacementFailedEffect()
    {
        Debug.LogWarning("無法放置到此位置");
        if (_worldDragInstance != null)
        {
            // 1. 先殺掉之前的震動防止疊加
            // 2. 使用 DOShakePosition 達成震動效果
            // 3. SetLink 確保物件銷毀時 Tween 自動停止，徹底解決 NRE
            _worldDragInstance.transform.DOShakePosition(0.3f, 0.2f, 10, 90)
                .SetLink(_worldDragInstance);

            // 可選：加入變紅效果
            var renderers = _worldDragInstance.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                r.material.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo).SetLink(_worldDragInstance);
            }
        }
    }
    #endregion
}
