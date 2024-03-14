using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBase.EventBus;
using UnityBase.Extensions;
using UnityBase.Manager.Data;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class GameplayManager : IGameplayDataService, IGameplayPresenterDataService
    {
        private readonly IGameDataService _gameDataService;
        private readonly ITutorialStepDataService _tutorialStepDataService;
        
        private GameState _startState = GameState.GameLoadingState;
        private GameState _currentGameState = GameState.None;
        public GameState CurrentGameState => _currentGameState;
        
        private bool _isTransitionStarted;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public GameplayManager(IGameDataService gameDataService, ITutorialStepDataService tutorialStepDataService, ISceneGroupLoadService sceneGroupLoadService)
        {
            _gameDataService = gameDataService;
            
            _tutorialStepDataService = tutorialStepDataService;
        }

        ~GameplayManager() => Dispose();

        public void Initialize()
        {
            
        }

        public void Start()
        {
            InitializeGameState();
        }

        public void Dispose()
        {
            DisposeToken();
            
            _startState = GameState.GameLoadingState;
            
            CinemachineManager.OnChangeCamera?.Invoke(_startState);
        }

        private void InitializeGameState()
        {
            if (_tutorialStepDataService.IsSelectedLevelTutorialEnabled)
            {
                TutorialStepManager.OnCompleteTutorialStep?.Invoke();
            }
            
            var gameState = _tutorialStepDataService.IsSelectedLevelTutorialEnabled ? GameState.GameTutorialState : GameState.GamePlayState;

            ChangeGameState(gameState, 1f, 0.5f);
        }

        public async void ChangeGameState(GameState gameState, float transitionDuration, float startDelay = 0f)
        {
            if (IsStateNotChangeable(gameState)) return;

            Debug.Log($"Changing state from {_startState} to {gameState}");

            _isTransitionStarted = true;

            _gameDataService?.PlayGame();

            GameStateData gameStateData = BuildGameStateData(gameState, transitionDuration);

            try
            {
                await ChangeStateAsync(gameStateData, transitionDuration, startDelay);
                
                _startState = gameState;
                
                _isTransitionStarted = false;
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e);
            }
        }

        private GameStateData BuildGameStateData(GameState gameState, float transitionDuration) =>
                new GameStateData.Builder().WithStartState(_startState)
                .WithEndState(gameState)
                .WithDuration(transitionDuration)
                .Build();

        private async UniTask ChangeStateAsync(GameStateData gameStateData, float transitionDuration, float startDelay)
        {
            CancellationTokenExtentions.Refresh(ref _cancellationTokenSource);
            
            var halfDuration = transitionDuration * 0.5f;
            
            await UniTask.WaitForSeconds(startDelay,false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            
            CinemachineManager.OnChangeCamera?.Invoke(gameStateData.EndState);
            InvokeStateData(gameStateData, TransitionState.Start);
            
            await UniTask.WaitForSeconds(halfDuration, false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            
            gameStateData.Duration = halfDuration;
            InvokeStateData(gameStateData, TransitionState.Middle);
            
            await UniTask.WaitForSeconds(halfDuration, false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            
            gameStateData.Duration = 0f;
            InvokeStateData(gameStateData, TransitionState.End);
        }

        private void InvokeStateData(GameStateData gameStateData, TransitionState transitionState)
        {
            EventBus<GameStateData>.Invoke(gameStateData, GameStateData.GetChannel(transitionState));
        }

        private bool IsStateNotChangeable(GameState nextGameplayState)
        {
            var isStatesAreSame = _startState == nextGameplayState;
            var isGameFailed = _startState == GameState.GameFailState && nextGameplayState == GameState.GameSuccessState;
            var isGameSuccess = _startState == GameState.GameSuccessState && nextGameplayState == GameState.GameFailState;
            var isTransitionNotCompleted = _isTransitionStarted;

            return isStatesAreSame || isGameFailed || isTransitionNotCompleted || isGameSuccess;
        }

        private void DisposeToken()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }

    public enum GameState { None = -1, GameLoadingState = 0, GamePauseState = 1, GameTutorialState = 2, GamePlayState = 3, GameFailState = 4, GameSuccessState = 5}
}