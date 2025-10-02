using UnityBase.Manager;

namespace UnityBase.UI.ButtonCore
{
    public class SceneLoadAction : ButtonActionBase
    {
        private readonly ISceneLoadService _sceneLoadService;
        private string _sceneName;
        private bool _useLoadingScene;
        
        public SceneLoadAction(ISceneLoadService sceneLoadService) => _sceneLoadService = sceneLoadService;

        public IButtonAction Configure(string sceneName, bool useLoadingScene)
        {
            _sceneName = sceneName;
            _useLoadingScene = useLoadingScene;
            return this;
        }
        
        public override void OnClick()
        {
            _sceneLoadService.LoadSceneAsync(_sceneName, _useLoadingScene);
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