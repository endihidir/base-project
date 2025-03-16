using UnityBase.Service;

namespace UnityBase.UI.ButtonCore
{
    public class SceneLoadAction : ButtonActionBase
    {
        private readonly ISceneGroupManager _sceneGroupManager;
        private SceneType _sceneType;
        private bool _useLoadingScene;
        private float _progressMultiplier;
        
        public SceneLoadAction(IButtonUI buttonUI, ISceneGroupManager sceneGroupManager) : base(buttonUI)
        {
            _sceneGroupManager = sceneGroupManager;
        }

        public IButtonAction Configure(SceneType sceneType, bool useLoadingScene, float progressMultiplier = 10f)
        {
            _sceneType = sceneType;
            _useLoadingScene = useLoadingScene;
            _progressMultiplier = progressMultiplier;
            return this;
        }
        
        public override void OnClick()
        {
            _sceneGroupManager.LoadSceneAsync(_sceneType, _useLoadingScene, _progressMultiplier);
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