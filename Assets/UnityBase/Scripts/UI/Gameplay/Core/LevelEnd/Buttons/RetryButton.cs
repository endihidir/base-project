using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBase.Extensions;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class RetryButton : ButtonBehaviour
{
    [SerializeField] private float _stateChangeDelay = 0.2f;

    [Inject]
    private readonly IGameDataService _gameDataService;
    
    [Inject] 
    private readonly ISceneGroupLoadService _sceneGroupLoadService;

    private CancellationTokenSource _cancellationTokenSource;

    protected override async void OnClick()
    {
        _gameDataService.PlayGame();

        CancellationTokenExtentions.Refresh(ref _cancellationTokenSource);

        await UniTask.Delay(TimeSpan.FromSeconds(_stateChangeDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
        
        _sceneGroupLoadService.LoadSceneAsync(SceneType.Gameplay, false, 2f);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}