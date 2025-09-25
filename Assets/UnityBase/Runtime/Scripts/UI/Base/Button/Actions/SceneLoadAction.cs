using UnityBase.Manager;

namespace UnityBase.UI.ButtonCore
{
    public class SceneLoadAction : ButtonActionBase
    {
        private readonly ISceneManager _sceneManager;
        private SceneType _sceneType;
        private bool _useLoadingScene;
        private float _progressMultiplier;
        
        public SceneLoadAction(ISceneManager sceneManager) => _sceneManager = sceneManager;

        public IButtonAction Configure(SceneType sceneType, bool useLoadingScene, float progressMultiplier = 10f)
        {
            _sceneType = sceneType;
            _useLoadingScene = useLoadingScene;
            _progressMultiplier = progressMultiplier;
            return this;
        }
        
        public override void OnClick()
        {
            _sceneManager.LoadSceneAsync(_sceneType, _useLoadingScene, _progressMultiplier);
        }
        
        public override void OnPointerDown()
        {
            
        }

        public override void OnPointerUp()
        {
            
        }

        public override void Dispose()
        {
            
        }
    }
}