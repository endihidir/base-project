using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.SceneManagement
{
    public class SceneGroupManager : ISceneManager, IAppBootService
    { 
        private bool _sceneLoadInProgress;
        private readonly SceneManagerSO _sceneManagerSo;
        private readonly ILoadingMenuActivator _loadingMenuActivator;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        private SceneReferenceState _sceneReferenceState;
        public event Action<SceneType> OnSceneLoaded;
        public LoadingProgress LoadingProgress { get; }
        public void Initialize() { }
        public void Dispose() { }
        
        public SceneGroupManager(GameDataHolderSO gameDataHolderSo)
        {
            _sceneManagerSo = gameDataHolderSo.sceneManagerSo;
            
            _loadingMenuActivator = _sceneManagerSo.LoadingMenuActivator;
            _loadingMenuActivator?.SetActive(false);
            
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
            
            LoadingProgress = new LoadingProgress();
        }

        public async UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene, float delayMultiplier = 10f)
        {
            if (_sceneLoadInProgress) return;
            
            _sceneLoadInProgress = true;
            
            if (useLoadingScene)
            {
                _loadingMenuActivator?.SetActive(true);
            }
            
            await UnloadSceneAsync();
            
            var sceneGroup = _sceneManagerSo.GetSceneData(sceneType);

            for (int i = 0; i < sceneGroup.Count; i++)
            {
                var sceneData = sceneGroup[i];

                _sceneReferenceState = sceneData.reference.State;
                
                if (sceneData.reference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                    
                    _operationGroup.Operations.Add(operation);
                }
                else if(sceneData.reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                    
                    _handleGroup.Handles.Add(sceneHandle);
                }
            }

            while (!_operationGroup.IsDone || !_handleGroup.IsDone)
            {
                LoadingProgress?.Report((_operationGroup.Progress + _handleGroup.Progress) / 1f);
                
                await UniTask.WaitForSeconds(0.1f * delayMultiplier);
            }

            if (useLoadingScene)
            {
                _loadingMenuActivator?.SetActive(false);
            }
            
            OnSceneLoaded?.Invoke(sceneType);
            
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