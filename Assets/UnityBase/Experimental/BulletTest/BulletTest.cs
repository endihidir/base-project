using System;
using DG.Tweening;
using UnityBase.Pool;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class BulletTest : MonoBehaviour, IPoolable
{
    private Tween _scaleTween;
    public Component PoolableObject => this;
    public bool IsActive => isActiveAndEnabled;
    public bool IsUnique => false;
    
    protected Collider _collider;

    [Inject]
    protected readonly IPoolDataService _poolDataService;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void Show(float duration, float delay, Action onComplete)
    {
        transform.localScale = Vector3.one;
        
        gameObject.SetActive(true);

        _collider.enabled = true;
        
        onComplete?.Invoke();
    }
    
    public void Hide(float duration, float delay, Action onComplete)
    {
        _scaleTween.Kill();
        
        _scaleTween = transform.DOScale(0f, duration).OnComplete(()=>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
    
    private void Update()
    {
        if(_scaleTween.IsActive()) return;
        
        transform.position += transform.forward * (Time.deltaTime * 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Tag_WallTest _))
        {
            _collider.enabled = false;
            
            _poolDataService.HideObject(this, 1f, 0f, default);
        }
    }

    private void OnDestroy()
    {
        _scaleTween.Kill();
    }
}