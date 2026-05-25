using DG.Tweening;
using UnityEngine;
using Zenject;

public class TilePointerView : MonoBehaviour
{
    private Tween _floatingTween;

    // 傳入 Vector3 作為生成時的世界座標
    public class Pool : MonoMemoryPool<Vector3, TilePointerView>
    {
        protected override void Reinitialize(Vector3 worldPosition, TilePointerView pointer)
        {
            // 將箭頭放在目標格子的中心，並稍微往上抬 (例如 +1f)
            float hoverOffset = 1.5f;
            pointer.transform.position = worldPosition + Vector3.up * hoverOffset;
            pointer.transform.localRotation = Quaternion.identity;
            pointer.PlaySpawnAnimation();
        }
    }

    public void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        _floatingTween?.Kill();
        // 原地上下浮動
        float startY = transform.position.y;
        _floatingTween = transform.DOMoveY(startY + 0.3f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void Despawn(Pool pool)
    {
        _floatingTween?.Kill();
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => pool.Despawn(this));
    }
}
