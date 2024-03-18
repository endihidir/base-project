using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class BulletSpawnerTest : MonoBehaviour
{
    [Inject] 
    private readonly IPoolDataService _poolDataService;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private void Start()
    {
        UpdateAsync();
    }

    private async void UpdateAsync()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var bullet = _poolDataService.GetObject<BulletTest>(true,0f,0f);

                    bullet.transform.SetParent(transform);
               
                    bullet.transform.localPosition = Vector3.zero;
                }

                await UniTask.Yield(_cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException e)
        {
            Debug.Log(e);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
