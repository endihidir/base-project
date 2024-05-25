using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityBase.EventBus;
using UnityBase.Manager.Data;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class GameManager : IGameDataService, IAppBootService
    {
        private CanvasGroup _splashScreen;
        
        private readonly ISceneManagementService _sceneManagementService;

        private bool _passSplashScreen;
        
        private Tween _splashTween;
        
        private EventBinding<GameStateData> _gameStateBinding = new EventBinding<GameStateData>();

        public GameManager(ManagerDataHolderSO managerDataHolderSo, ISceneManagementService sceneManagementService)
        {
            var gameManagerData = managerDataHolderSo.gameManagerSo;
            _splashScreen = gameManagerData.splashScreen;
            _sceneManagementService = sceneManagementService;
            _passSplashScreen = gameManagerData.passSplashScreen;
            
            Application.targetFrameRate = gameManagerData.targetFrameRate;
            Input.multiTouchEnabled = gameManagerData.isMultiTouchEnabled;
        }

        ~GameManager() => Dispose();
        
        public void Initialize() => LoadGame();
        public void Dispose() => _splashTween.Kill();

        private async void LoadGame()
        {
            if (!_passSplashScreen) await StartSplashScreen();

            _sceneManagementService.LoadSceneAsync(SceneType.MainMenu);
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
    }
}