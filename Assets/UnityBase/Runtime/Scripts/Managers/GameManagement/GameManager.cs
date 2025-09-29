using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityBase.BaseLifetimeScope;
using UnityBase.GameDataHolder;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Manager
{
    public interface IGameManager
    {
        void Initialize();
        void Dispose();
    }
    
    public class GameManager : IGameManager
    {
        private CanvasGroup _splashScreen;
        
        private readonly ISceneManager _sceneManager;

        private bool _passSplashScreen;
        
        private Tween _splashTween;
        
        public IObjectResolver ObjectResolver { get; }

        public GameManager(GameDataHolderSO gameDataHolderSo, ISceneManager sceneManager)
        {
            var gameManagerData = gameDataHolderSo.gameManagerSo;
            _splashScreen = gameManagerData.splashScreen;
            _sceneManager = sceneManager;
            _passSplashScreen = gameManagerData.passSplashScreen;
            
            Application.targetFrameRate = gameManagerData.targetFrameRate;
            Input.multiTouchEnabled = gameManagerData.isMultiTouchEnabled;
        }

        ~GameManager() => Dispose();

        public void Initialize() => LoadGame().Forget();
        public void Dispose() => _splashTween.Kill();

        private async UniTask LoadGame()
        {
            if (!_passSplashScreen) await StartSplashScreen();
            
            await _sceneManager.LoadSceneAsync(SceneType.MainMenu);
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