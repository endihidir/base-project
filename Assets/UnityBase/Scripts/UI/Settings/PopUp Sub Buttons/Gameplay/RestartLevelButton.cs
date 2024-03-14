using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityBase.Extensions;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class RestartLevelButton : ButtonBehaviour
{
    [SerializeField] private float _stateChangeDelay = 0.2f;

    [Inject]
    private readonly IGameDataService _gameDataService;
    
    [Inject] 
    private readonly ISceneGroupLoadService _sceneGroupLoadService;

    [Inject] 
    protected readonly IPopUpDataService _popUpDataService;
    
    [Inject] 
    protected readonly ITutorialStepDataService _tutorialStepDataService;
    
    private CancellationTokenSource _cancellationTokenSource;
    
    [ReadOnly] [SerializeField] private PopUp _popUp;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        _popUp = GetComponentInParent<PopUp>(true);
    }
#endif
    
    protected override async void OnClick()
    {
        _gameDataService.PlayGame();

        _popUpDataService.HidePopUp(_popUp);

        _tutorialStepDataService.ResetGamePlayTutorial();

        CancellationTokenExtentions.Refresh(ref _cancellationTokenSource);

        await UniTask.Delay(TimeSpan.FromSeconds(_stateChangeDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
        
        _sceneGroupLoadService.LoadSceneAsync(SceneType.Gameplay, true, 5f);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}