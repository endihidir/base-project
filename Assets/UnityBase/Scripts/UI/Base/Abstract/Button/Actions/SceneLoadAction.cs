using UnityBase.Service;

namespace UnityBase.UI.ButtonCore
{
    public class SceneLoadAction : ButtonAction
    {
        private readonly ISceneManagementService _sceneManagementService;
        
        private SceneType _sceneType;
        private bool _useLoadingScene;
        private float _progressMultiplier;
        
        public SceneLoadAction(IButtonUI buttonUI, ISceneManagementService sceneManagementService) : base(buttonUI)
        {
            _sceneManagementService = sceneManagementService;
        }

        public void Configure(SceneType sceneType, bool useLoadingScene, float progressMultiplier)
        {
            _sceneType = sceneType;
            _useLoadingScene = useLoadingScene;
            _progressMultiplier = progressMultiplier;
        }
        
        public override void OnClick()
        {
            _sceneManagementService.LoadSceneAsync(_sceneType, _useLoadingScene, _progressMultiplier);
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