using Cysharp.Threading.Tasks;
using UnityBase.Manager;

namespace UnityBase.Runtime.Factories
{
    public class LoadingPresenter : ILoadingPresenter
    {
        private readonly ISceneLoadService _sceneLoadService;
        private ILoadingModel _model;
        private ILoadingView _view;

        public LoadingPresenter(ISceneLoadService sceneLoadService) => _sceneLoadService = sceneLoadService;

        public ILoadingPresenter Initialize(ILoadingModel model, ILoadingView view)
        {
            _model = model;
            _view = view;

            _sceneLoadService.OnBeforeSceneLoad += HandleBeforeSceneLoad;
            _sceneLoadService.LoadingProgress.Progressed += HandleProgressed;
            _sceneLoadService.OnAfterScenesLoad += HandleAfterScenesLoadAsync;

            return this;
        }

        public void StartLoadingProgress(float ratio) => _model.SetTargetRatio(ratio);

        public void ResetProgress() => _model.ResetProgress();
        
        public UniTask SetActive(bool value) => _view.SetActive(value);

        public void Update()
        {
            _model.UpdateData();
            _view.SetFillAmount(_model.FillAmount);

            var percentage = _model.FillAmount * 100f;
            _view.SetText("Loading... " + percentage.ToString("0.0") + "%");
        }
        
        private void HandleBeforeSceneLoad(bool useLoadingScene, string sceneName)
        {
            if (!useLoadingScene) return;

            ResetProgress();
            SetActive(true).Forget();
        }

        private void HandleProgressed(float ratio)
        {
            StartLoadingProgress(ratio);
        }

        private UniTask HandleAfterScenesLoadAsync(bool useLoadingScene, string sceneName)
        {
            return !useLoadingScene ? UniTask.CompletedTask : SetActive(false);
        }
        
        public void Dispose()
        {
            _sceneLoadService.OnBeforeSceneLoad -= HandleBeforeSceneLoad;
            _sceneLoadService.LoadingProgress.Progressed -= HandleProgressed;
            _sceneLoadService.OnAfterScenesLoad -= HandleAfterScenesLoadAsync;
        }
    }
    
    public interface ILoadingPresenter : IPresenter, IUpdater
    {
        public ILoadingPresenter Initialize(ILoadingModel model, ILoadingView view);
        public void StartLoadingProgress(float ratio);
        public void ResetProgress();
        public UniTask SetActive(bool value);
    }
}
