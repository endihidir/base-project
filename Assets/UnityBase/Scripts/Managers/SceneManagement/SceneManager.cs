using System;
using Cysharp.Threading.Tasks;
using UnityBase.Controller;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.SceneManagement
{
    public class SceneManager : ISceneDataService
    { 
        public event Action<float> OnLoadUpdate;

        private bool _sceneLoadInProgress;
        private float _currentProgressValue, _targetProgressValue;

        private readonly SceneManagerSO _sceneManagerSo;
        private readonly LoadingSceneController _loadingSceneController;
        private readonly AsyncOperationHandleGroup _handleGroup;

        public SceneManager(ManagerDataHolderSO managerDataHolderSo)
        {
            _sceneManagerSo = managerDataHolderSo.sceneManagerSo;
            _loadingSceneController = new LoadingSceneController(_sceneManagerSo.loadingSceneAssetSo.sceneData);
            _handleGroup = new AsyncOperationHandleGroup(10);
        }

        public async UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene, float progressMultiplier)
        {
            if (_sceneLoadInProgress) return;
            
            _sceneLoadInProgress = true;
            
            await UnloadSceneAsync();
            
            if (useLoadingScene)
            {
                await _loadingSceneController.Initialize();
            }
            
            var sceneGroup = _sceneManagerSo.GetSceneData(sceneType);
            
            for (int i = 0; i < sceneGroup.Count; i++)
            {
                var sceneHandle = Addressables.LoadSceneAsync(sceneGroup[i].reference.Path, LoadSceneMode.Additive, false);
                
                _handleGroup.Handles.Add(sceneHandle);
                
                await UniTask.WaitUntil(() => sceneHandle.IsDone);
            }
            
            await WaitProgress(progressMultiplier,0.1f);

            await _handleGroup.ActivateResultsAsync();
            
            if (useLoadingScene)
            {
                await _loadingSceneController.ReleaseLoadingScene();
            }
            
            _sceneLoadInProgress = false;
        }
        
        private async UniTask UnloadSceneAsync()
        {
            foreach (var handle in _handleGroup.Handles) 
            {
                if (!handle.IsValid()) continue;
                
                await Addressables.UnloadSceneAsync(handle);
            }

            _handleGroup.Handles.Clear();
            
            await UniTask.WaitUntil(()=> _handleGroup.IsDone);

            await Resources.UnloadUnusedAssets();
        }

        private async UniTask WaitProgress(float progressMultiplier, float delay)
        {
            _currentProgressValue = 0;
            _targetProgressValue = _handleGroup.Progress / 0.9f;

            while (!Mathf.Approximately(_currentProgressValue, _targetProgressValue))
            {
                _currentProgressValue = Mathf.MoveTowards(_currentProgressValue, _targetProgressValue, progressMultiplier * Time.deltaTime);
                
                OnLoadUpdate?.Invoke(_currentProgressValue);
                
                await UniTask.WaitForSeconds(delay);
            }
        }
    }
}