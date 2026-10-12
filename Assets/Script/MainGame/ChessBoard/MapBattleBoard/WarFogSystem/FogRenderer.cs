using UnityEngine;

public interface IFogRenderer
{
    // 初始化迷霧畫布大小
    void InitializeFogMap(int width, int height);

    // 接收一維陣列 (true=可見, false=迷霧)，進行渲染
    void RenderFog(bool[] visibilityMap);

    // 清除迷霧 (全亮)
    void ClearFog();

    // 釋放資源
    void DisposeMap();
}

public class FogRenderer : IFogRenderer
{
    private Texture2D _fogMapTexture;
    private Color32[] _fogPixels;

    private int _width;
    private int _height;


    public void DisposeMap()
    {
        if (_fogMapTexture != null)
        {
            Object.Destroy(_fogMapTexture);
        }
    }


    public void InitializeFogMap(int width, int height)
    {
        _width = width;
        _height = height;
        _fogMapTexture = new Texture2D(width, height, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        _fogPixels = new Color32[width * height];
    }

    public void RenderFog(bool[] visibilityMap)
    {
        if (visibilityMap.Length != _fogPixels.Length) return;

        for (int i = 0; i < visibilityMap.Length; i++)
        {
            // true = 白色(可見), false = 黑色(迷霧)
            _fogPixels[i] = visibilityMap[i] ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 255);
        }
        ApplyTexture();
    }

    public void ClearFog()
    {
        if (_fogPixels == null) return;
        for (int i = 0; i < _fogPixels.Length; i++)
        {
            _fogPixels[i] = new Color32(255, 255, 255, 255);
        }
        ApplyTexture();
    }

    private void ApplyTexture()
    {
        if (_fogMapTexture == null) return;
        _fogMapTexture.SetPixels32(_fogPixels);
        _fogMapTexture.Apply();
        Shader.SetGlobalTexture("_FogMap", _fogMapTexture);
    }
}
