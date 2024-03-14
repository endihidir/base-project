using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.ManagerSO;
using UnityBase.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.Controller
{
    public class LoadingSceneController
    {
        private SceneData _loadingScene;

        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;

        private SceneReferenceState _sceneReferenceState;

        public LoadingSceneController(SceneData loadingScene)
        {
            _loadingScene = loadingScene;
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
        }
        
        public async UniTask Initialize()
        {
            _sceneReferenceState = _loadingScene.reference.State;
            
            if (_sceneReferenceState == SceneReferenceState.Regular)
            {
                var operation = SceneManager.LoadSceneAsync(_loadingScene.reference.Path, LoadSceneMode.Additive);

                _operationGroup.Operations.Add(operation);

                await UniTask.WaitUntil(() => operation.isDone);
            }
            else if (_sceneReferenceState == SceneReferenceState.Addressable)
            {
                var handle = Addressables.LoadSceneAsync(_loadingScene.reference.Path, LoadSceneMode.Additive);

                _handleGroup.Handles.Add(handle);

                await UniTask.WaitUntil(() => _handleGroup.IsDone);
            }
        }

        public async UniTask ReleaseLoadingScene()
        {
            if (_sceneReferenceState == SceneReferenceState.Regular)
            {
                await SceneManager.UnloadSceneAsync(_loadingScene.Name);
                
                _operationGroup.Operations.Clear();
                
            }
            else if (_sceneReferenceState == SceneReferenceState.Addressable)
            {
                foreach (var handle in _handleGroup.Handles)
                {
                    if (handle.IsValid())
                    {
                        await Addressables.UnloadSceneAsync(handle);
                    }
                }
            
                _handleGroup.Handles.Clear();
            }
            
            await Resources.UnloadUnusedAssets();
        }
    }
}