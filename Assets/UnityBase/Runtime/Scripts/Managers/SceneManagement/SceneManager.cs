using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.BootService;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.SceneManagement
{
    public class SceneManager : ISceneManager, IAppBootService
    { 
        private bool _sceneLoadInProgress;
        private readonly SceneGroupManagerSO _sceneGroupManagerSo;
        private readonly ILoadingMenuController _loadingMenuController;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        private SceneReferenceState _sceneReferenceState;
        public event Action<SceneType> OnBeforeSceneLoad;
        public event Action<SceneType> OnSceneReady;
        public event Action<SceneType> OnSceneReadyToPlay;
        public LoadingProgress LoadingProgress { get; }
        public void Initialize() { }
        public void Dispose() { }
        
        public SceneManager(GameDataHolderSO gameDataHolderSo)
        {
            _sceneGroupManagerSo = gameDataHolderSo.sceneGroupManagerSo;
            
            _loadingMenuController = _sceneGroupManagerSo.LoadingMenuController;
            _loadingMenuController.SetActive(false);
            
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
                _loadingMenuController.Reset();
                await _loadingMenuController.SetActive(true);
            }
            
            OnBeforeSceneLoad?.Invoke(sceneType);
            
            await UnloadSceneAsync();
            
            var sceneGroup = _sceneGroupManagerSo.GetSceneData(sceneType);

            for (int i = 0; i < sceneGroup.Count; i++)
            {
                var sceneData = sceneGroup[i];

                _sceneReferenceState = sceneData.reference.State;
                
                if (sceneData.reference.State == SceneReferenceState.Regular)
                {
                    var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                    
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
            
            OnSceneReady?.Invoke(sceneType);

            if (useLoadingScene)
            {
                await _loadingMenuController.SetActive(false);
            }
            
            OnSceneReadyToPlay?.Invoke(sceneType);
            
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

                    await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                }

                _operationGroup.Operations.Clear();
            }
            
            await Resources.UnloadUnusedAssets();
        }

        private List<string> GetScenes()
        {
            var scenes = new List<string>();
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (var i = sceneCount - 1; i > 0; i--) 
            {
                var sceneAt = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;
                
                var sceneName = sceneAt.name;
                if (sceneName.Equals(activeScene)) continue;

                scenes.Add(sceneName);
            }

            return scenes;
        }
    }
}