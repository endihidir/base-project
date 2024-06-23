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
    public class GameplayManager : IGameplayManager, IGameplayBootService
    {
        private readonly ISceneManager _sceneManager;
        private readonly ITutorialProcessManager _tutorialProcessManager;
        
        private GameState _currentGameState = GameState.GameLoadingState;
        public GameState CurrentGameState => _currentGameState;
        
        private bool _isTransitionStarted;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public GameplayManager(ISceneManager sceneManager, ITutorialProcessManager tutorialProcessManager)
        {
            _sceneManager = sceneManager;
            _tutorialProcessManager = tutorialProcessManager;
        }

        ~GameplayManager() => Dispose();

        public void Initialize()
        {
            _sceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(SceneType sceneType)
        {
            if (sceneType == SceneType.Gameplay)
            {
                InitializeGameState();
            }
        }

        public void Dispose()
        {
            _sceneManager.OnSceneLoaded -= OnSceneLoaded;
            
            DisposeToken();
            
            _currentGameState = GameState.GameLoadingState;
            
            CinemachineManager.OnChangeCamera?.Invoke(_currentGameState);
        }

        private void InitializeGameState()
        {
            if (_tutorialProcessManager.IsSelectedLevelTutorialEnabled)
            {
                TutorialProcessManager.OnCompleteTutorialStep?.Invoke();
            }
            
            var gameState = _tutorialProcessManager.IsSelectedLevelTutorialEnabled ? GameState.GameTutorialState : GameState.GamePlayState;

            ChangeGameState(gameState, 1f);
        }

        public async void ChangeGameState(GameState nextGameState, float transitionDuration = 0f, float startDelay = 0f)
        {
            if (IsStateNotChangeable(nextGameState)) return;

            Debug.Log($"Changing state from {_currentGameState} to {nextGameState}");

            _isTransitionStarted = true;

            GameStateData gameStateData = BuildGameStateData(nextGameState, transitionDuration);

            try
            {
                await ChangeStateAsync(gameStateData, transitionDuration, startDelay);
                
                _currentGameState = nextGameState;
                
                _isTransitionStarted = false;
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e);
            }
        }

        private GameStateData BuildGameStateData(GameState gameState, float transitionDuration) =>
                new GameStateData.Builder().WithStartState(_currentGameState)
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
            var isStatesAreSame = _currentGameState == nextGameplayState;
            var isGameFailed = _currentGameState == GameState.GameFailState && nextGameplayState == GameState.GameSuccessState;
            var isGameSuccess = _currentGameState == GameState.GameSuccessState && nextGameplayState == GameState.GameFailState;
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