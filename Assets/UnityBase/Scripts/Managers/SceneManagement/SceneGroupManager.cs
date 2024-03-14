using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.Controller;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.SceneManagement
{
    public class SceneGroupManager : ISceneGroupLoadService
    { 
        public event Action<float> OnLoadUpdate;

        private bool _sceneLoadInProgress;
        private readonly SceneManagerSO _sceneManagerSo;
        private readonly LoadingSceneController _loadingSceneController;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        private SceneReferenceState _sceneReferenceState;

        public SceneGroupManager(ManagerDataHolderSO managerDataHolderSo)
        {
            _sceneManagerSo = managerDataHolderSo.sceneManagerSo;
            
            _loadingSceneController = new LoadingSceneController(_sceneManagerSo.loadingSceneAssetSo.sceneData);
            
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
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
                var sceneData = sceneGroup[i];

                _sceneReferenceState = sceneData.reference.State;
                
                if (sceneData.reference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);

                    operation.allowSceneActivation = false;
                    
                    _operationGroup.Operations.Add(operation);
                }
                else if(sceneData.reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive, false);
                    
                    _handleGroup.Handles.Add(sceneHandle);
                    
                    await UniTask.WaitUntil(() => sceneHandle.IsDone);
                }
            }

            await WaitProgress(progressMultiplier, 0.1f);
            
            if (_sceneReferenceState == SceneReferenceState.Regular)
            {
                _operationGroup.Operations.ForEach(x => x.allowSceneActivation = true);

                await UniTask.WaitUntil(() => _operationGroup.IsDone);
            }
            else if (_sceneReferenceState == SceneReferenceState.Addressable)
            {
                await _handleGroup.ActivateResultsAsync();
            }

            if (useLoadingScene)
            {
                await _loadingSceneController.ReleaseLoadingScene();
            }
            
            _sceneLoadInProgress = false;
        }
        
        private async UniTask UnloadSceneAsync()
        {
            if (_sceneReferenceState == SceneReferenceState.Addressable)
            {
                foreach (var handle in _handleGroup.Handles) 
                {
                    if (!handle.IsValid()) continue;
                    
                    await Addressables.UnloadSceneAsync(handle);
                }

                _handleGroup.Handles.Clear();
            }
            else if (_sceneReferenceState == SceneReferenceState.Regular)
            {
                foreach (var scene in GetScenes())
                {
                    if(scene == null) continue;

                    await SceneManager.UnloadSceneAsync(scene);
                }

                _operationGroup.Operations.Clear();
            }
            
            await Resources.UnloadUnusedAssets();
        }

        private async UniTask WaitProgress(float progressMultiplier, float delay)
        {
            var currentProgressValue = 0f;
            
            var progress = _sceneReferenceState == SceneReferenceState.Regular ? 1f : _handleGroup.Progress;

            var targetProgressValue = progress / 0.9f;

            while (!Mathf.Approximately(currentProgressValue, targetProgressValue))
            {
                currentProgressValue = Mathf.MoveTowards(currentProgressValue, targetProgressValue, progressMultiplier * Time.deltaTime);
                
                OnLoadUpdate?.Invoke(currentProgressValue);
                
                await UniTask.WaitForSeconds(delay);
            }
        }

        private List<string> GetScenes()
        {
            var scenes = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;

            int sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i > 0; i--) 
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;
                
                var sceneName = sceneAt.name;
                if (sceneName.Equals(activeScene)) continue;

                scenes.Add(sceneName);
            }

            return scenes;
        }
    }
}