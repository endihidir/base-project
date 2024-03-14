using Cysharp.Threading.Tasks;
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

        public LoadingSceneController(SceneData loadingScene)
        {
            _loadingScene = loadingScene;

            _handleGroup = new AsyncOperationHandleGroup(10);
        }
        
        public async UniTask Initialize()
        {
            var handle = Addressables.LoadSceneAsync(_loadingScene.reference.Path, LoadSceneMode.Additive);

            _handleGroup.Handles.Add(handle);

            await UniTask.WaitUntil(() => _handleGroup.IsDone);
        }

        public async UniTask ReleaseLoadingScene()
        {
            foreach (var handle in _handleGroup.Handles)
            {
                if (handle.IsValid())
                {
                    await Addressables.UnloadSceneAsync(handle);
                }
            }
            
            _handleGroup.Handles.Clear();
            
            await UniTask.WaitUntil(()=> _handleGroup.IsDone);

            await Resources.UnloadUnusedAssets();
        }
    }
}