using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class LevelEndMainMenuButton : ButtonBehaviour
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

        _cancellationTokenSource = new CancellationTokenSource();

        await UniTask.Delay(TimeSpan.FromSeconds(_stateChangeDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            
        _sceneGroupLoadService.LoadSceneAsync(SceneType.MainMenu, true, 5f);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}