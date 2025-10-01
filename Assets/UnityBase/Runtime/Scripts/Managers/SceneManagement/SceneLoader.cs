using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.Extensions;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityBase.Manager
{
    public interface ISceneLoader
    {
        public event Action<SceneType> OnBeforeSceneLoad;
        public event Action<SceneType> OnSceneReady; 
        public event Action<SceneType> OnSceneReadyToPlay;
        public LoadingProgress LoadingProgress { get; }
        public UniTask EnsureBootSceneAsync();
        public UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, float delayMultiplier = 10f);
    }
    public class SceneLoader : ISceneLoader
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
        
        public SceneLoader(GameDataHolderSO gameDataHolderSo)
        {
            _sceneGroupManagerSo = gameDataHolderSo.sceneGroupManagerSo;
            
            _loadingMenuController = _sceneGroupManagerSo.LoadingMenuController;
            
            _loadingMenuController.SetActive(false);
            
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
            
            LoadingProgress = new LoadingProgress();
        }
        
        public async UniTask EnsureBootSceneAsync()
        {
            var current = SceneManager.GetActiveScene().name;
            
            var boot = BuildSettingsUtil.GetFirstBuildSceneName();
            
            if (!string.IsNullOrEmpty(boot) && current != boot)
            {
                await SceneManager.LoadSceneAsync(boot, LoadSceneMode.Single);
            }
        }

        public async UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, float delayMultiplier = 10f)
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
                if (useLoadingScene)
                {
                    var realProgress = (_operationGroup.Progress + _handleGroup.Progress) / 1f;
                    LoadingProgress?.Report(realProgress <= 0f ? 1f : realProgress);
                }
                
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