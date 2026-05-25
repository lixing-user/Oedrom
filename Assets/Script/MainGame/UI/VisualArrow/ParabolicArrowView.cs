using UnityEngine;

public class ParabolicArrowView : MonoBehaviour
{
    [Header("視覺元件")]
    public LineRenderer LineRenderer;
    public Transform ArrowHead; // 箭頭的錐體

    [Header("曲線參數")]
    [Tooltip("拋物線的高度")]
    public float ArcHeight = 2.0f;
    [Tooltip("線段的平滑度 (頂點數)")]
    public int Resolution = 20;

    [Header("動態特效")]
    [Tooltip("材質 UV 滾動的速度")]
    public float UvScrollSpeed = 1.0f;

    [Header("地形貼齊設定")]
    [Tooltip("地形的 Layer，用於射線檢測高度")]
    public LayerMask SurfaceLayerMask;
    [Tooltip("箭頭起終點距離地面的額外高度")]
    public float HeightOffset = 0.5f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Material _lineMaterial;

    private void Awake()
    {
        if (LineRenderer != null)
        {
            _lineMaterial = LineRenderer.material;
        }

        if (SurfaceLayerMask == 0) SurfaceLayerMask = LayerMask.GetMask("Terrain", "Board");
    }

    // 由 Service 呼叫：初始化箭頭的起點與終點
    public void Setup(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        _startPos = AdjustHeightToTerrain(startWorldPos) + Vector3.up * HeightOffset;
        _endPos = AdjustHeightToTerrain(endWorldPos) + Vector3.up * HeightOffset;

        LineRenderer.positionCount = Resolution;
        DrawArc();
    }

    private Vector3 AdjustHeightToTerrain(Vector3 flatPosition)
    {
        Vector3 rayStart = new Vector3(flatPosition.x, 100f, flatPosition.z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, SurfaceLayerMask))
        {
            return hit.point; // 回傳打到地形的真實 3D 座標
        }

        // 如果沒打到（例如場景外），就回傳原本的座標
        return flatPosition;
    }

    private void DrawArc()
    {
        // 計算控制點 (Control Point)，決定拋物線的頂點
        Vector3 midPoint = (_startPos + _endPos) / 2f;
        Vector3 controlPoint = midPoint + Vector3.up * ArcHeight;

        // 計算貝茲曲線上的所有頂點
        for (int i = 0; i < Resolution; i++)
        {
            float t = i / (float)(Resolution - 1);
            Vector3 pointOnCurve = CalculateBezierPoint(t, _startPos, controlPoint, _endPos);
            LineRenderer.SetPosition(i, pointOnCurve);
        }

        // 處理箭頭頭部 (ArrowHead) 的位置與旋轉
        if (ArrowHead != null)
        {
            // 將箭頭放在最後一個點
            ArrowHead.position = _endPos;

            // 計算最後一小段的方向，讓箭頭指向終點
            Vector3 direction = (_endPos - LineRenderer.GetPosition(Resolution - 2)).normalized;
            if (direction != Vector3.zero)
            {
                ArrowHead.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    // 二次貝茲曲線公式
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2;        // t^2 * P2

        return p;
    }

    private void Update()
    {
        // 讓箭頭的材質 UV 滾動，產生流動感
        if (_lineMaterial != null)
        {
            float offset = Time.time * UvScrollSpeed;
            _lineMaterial.mainTextureOffset = new Vector2(-offset, 0);
        }
    }
}
