using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityBase.EventBus;
using UnityBase.Manager.Data;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class GameManager : IGameDataService, IAppPresenterDataService
    {
        private CanvasGroup _splashScreen;
        private readonly ISceneGroupLoadService _sceneGroupLoadService;
        private float _fixedDeltaTime;
        private bool _passSplashScreen;
        private Tween _splashTween;
        
        private EventBinding<GameStateData> _gameStateBinding = new EventBinding<GameStateData>();

        public GameManager(ManagerDataHolderSO managerDataHolderSo, ISceneGroupLoadService sceneGroupLoadService)
        {
            var gameManagerData = managerDataHolderSo.gameManagerSo;
            _splashScreen = gameManagerData.splashScreen;
            _sceneGroupLoadService = sceneGroupLoadService;
            _passSplashScreen = gameManagerData.passSplashScreen;
            
            Application.targetFrameRate = gameManagerData.targetFrameRate;
            Input.multiTouchEnabled = gameManagerData.isMultiTouchEnabled;
        }

        ~GameManager() => Dispose();

        public void Initialize()
        {
            PlayGame();
            
            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        public void Start()
        {
            _gameStateBinding.Add(OnStartGameStateTransition);
            
            EventBus<GameStateData>.AddListener(_gameStateBinding, GameStateData.GetChannel(TransitionState.Start));
            
            LoadGame();
        }
        
        public void Dispose()
        {
            _gameStateBinding.Remove(OnStartGameStateTransition);
            
            EventBus<GameStateData>.RemoveListener(_gameStateBinding, GameStateData.GetChannel(TransitionState.Start));
            
            _splashTween.Kill();
        }

        private void OnStartGameStateTransition(GameStateData gameStateData)
        {
            if (gameStateData.EndState == GameState.GamePauseState)
                PauseGame();
            else
                PlayGame();
        }

        private async void LoadGame()
        {
            if (!_passSplashScreen) await StartSplashScreen();

            _sceneGroupLoadService.LoadSceneAsync(SceneType.MainMenu);
        }

        private async UniTask StartSplashScreen()
        {
            if (!_splashScreen) return;
            
            _splashScreen.gameObject.SetActive(true);

            await UniTask.WaitForSeconds(3.5f);

            _splashTween = _splashScreen.DOFade(0f, 0.25f).SetEase(Ease.Linear)
                                        .OnComplete(() => _splashScreen.gameObject.SetActive(false));
            
            await UniTask.WaitForSeconds(0.25f);
        }

        public void PlayGame()
        {
            if (Time.timeScale > 0f) return;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
        }

        public void PauseGame()
        {
            if(Time.timeScale < 1f) return;
            Time.timeScale = 0;
            Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
        }
    }
}