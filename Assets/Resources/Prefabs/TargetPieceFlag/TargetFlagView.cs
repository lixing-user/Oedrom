using DG.Tweening;
using UnityEngine;
using Zenject;

public class TargetFlagView : MonoBehaviour
{
    private Tween _floatingTween;

    // 定義 Zenject 的 MemoryPool
    public class Pool : MonoMemoryPool<Transform, TargetFlagView>
    {
        protected override void Reinitialize(Transform parent, TargetFlagView flag)
        {
            flag.transform.SetParent(parent);
            flag.transform.localPosition = new Vector3(0, 2.5f, 0); // 在棋子頭頂
            flag.transform.localRotation = Quaternion.identity;
            flag.PlaySpawnAnimation();
        }
    }

    public void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 上下浮動待機動畫
        _floatingTween?.Kill();
        _floatingTween = transform.DOLocalMoveY(2.8f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void Despawn(Pool pool)
    {
        _floatingTween?.Kill();
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => pool.Despawn(this)); // 動畫播完後回收進水池
    }
}
