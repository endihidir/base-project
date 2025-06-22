using System;
using DG.Tweening;
using UnityBase.Pool;
using UnityEngine;

public class CoinIconTest : MonoBehaviour, IPoolable
{
    public bool IsActive => isActiveAndEnabled;
    public bool IsUnique => false;
    public int PoolKey { get; set; }

    private Tween _tw;
    
    public void Show(float duration, float delay, Action onComplete)
    {
        gameObject.SetActive(true);
        
        onComplete?.Invoke();
    }

    public void Hide(float duration, float delay, Action onComplete)
    {
        gameObject.SetActive(false);
        
        onComplete?.Invoke();
    }

    public void MoveTo(Transform target, float duration, Action onMoveComplete, Action onAnimComplete)
    {
        _tw?.Kill(true);

        _tw = DOTween.Sequence()
            .Append(transform.DOMove(target.position, duration).SetEase(Ease.InOutQuad))
            .Join(transform.DOScale(1f, duration).SetEase(Ease.InOutQuad))
            .AppendCallback(()=> onMoveComplete?.Invoke())
            .Append(transform.DOPunchScale(Vector3.one * 0.5f, 0.1f).SetEase(Ease.InBack))
            .AppendCallback(() => onAnimComplete?.Invoke());
    }
}
