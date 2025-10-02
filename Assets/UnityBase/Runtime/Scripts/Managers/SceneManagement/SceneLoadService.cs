using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityBase.Extensions;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UnityBase.Manager
{
    public interface ISceneLoadService
    {
        public event Action<bool, string> OnBeforeSceneLoad;
        public event Func<bool, string, UniTask>  OnAfterScenesLoad; 
        public event Action<string> OnSceneLoad; 
        public event Action<string> OnScenesReady;
        public event Action<string> OnSceneUnloaded; 
        public LoadingProgress LoadingProgress { get; }
        public bool IsInStartScene { get; }
        public UniTask EnsureBootSceneAsync();
        public UniTask LoadSceneAsync(string sceneName, bool useLoadingScene = false, bool reloadDupScenes = false);
    }
    public class SceneLoadService : ISceneLoadService
    { 
        private readonly SceneLoaderSO _sceneLoaderSo;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        public event Action<bool, string> OnBeforeSceneLoad;
        public event Func<bool, string, UniTask> OnAfterScenesLoad;
        public event Action<string> OnSceneLoad; 
        public event Action<string> OnScenesReady;
        public event Action<string> OnSceneUnloaded;
        public LoadingProgress LoadingProgress { get; }
        public bool IsInStartScene 
        {
            get
            {
                var current = SceneManager.GetActiveScene().name;
                
                var firstScene = BuildSettingsUtil.GetFirstBuildSceneName();
                
                return current == firstScene;
            }  
        }
        
        public SceneLoadService(GameDataHolderSO gameDataHolderSo)
        {
            _sceneLoaderSo = gameDataHolderSo.sceneLoaderSo;
            
            _handleGroup = new AsyncOperationHandleGroup(10);
            
            _operationGroup = new AsyncOperationGroup(10);
            
            LoadingProgress = new LoadingProgress();
        }
        
        public async UniTask EnsureBootSceneAsync()
        {
            var current = SceneManager.GetActiveScene().name;

            var firstScene = BuildSettingsUtil.GetFirstBuildSceneName();
            
            if (current != firstScene)
            {
                await SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Single);
            }
        }

        public async UniTask LoadSceneAsync(string sceneName, bool useLoadingScene = false, bool reloadDupScenes = false)
        {
            var loadedScenes = new List<string>();
            
            OnBeforeSceneLoad?.Invoke(useLoadingScene, sceneName);
            
            var sceneCount = SceneManager.sceneCount;
            
            for (var i = 0; i < sceneCount; i++) 
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }
            
            await UnloadSceneAsync();
            
            var sceneGroup = _sceneLoaderSo.GetSceneData(sceneName);

            for (int i = 0; i < sceneGroup.Count; i++)
            {
                var sceneData = sceneGroup[i];
                
                if (!reloadDupScenes && loadedScenes.Contains(sceneData.Name)) continue;
       
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
                
                OnSceneLoad?.Invoke(sceneData.Name);
            }
            
            while (!_operationGroup.IsDone || !_handleGroup.IsDone)
            {
                var avg = CombinedProgress(_operationGroup, _handleGroup);
                
                LoadingProgress?.Report(avg);
                
                await UniTask.Delay(100);
            }

            var activeScene = SceneManager.GetSceneByName(sceneName);

            if (activeScene.IsValid()) 
            {
                SceneManager.SetActiveScene(activeScene);
            }
            
            LoadingProgress?.Report(1f);
            
            if (OnAfterScenesLoad != null)
            {
                await InvokeAfterScenesLoadAll(useLoadingScene, sceneName);
            }

            OnScenesReady?.Invoke(sceneName);
        }
        
        private async UniTask InvokeAfterScenesLoadAll(bool useUI, string sceneName)
        {
            var list = OnAfterScenesLoad?.GetInvocationList();
            
            if (list == null || list.Length == 0) return;

            var tasks = new UniTask[list.Length];
            
            for (int i = 0; i < list.Length; i++)
            {
                var fn = (Func<bool, string, UniTask>)list[i];
                tasks[i] = SafeCall(fn, useUI, sceneName);
            }
            
            await UniTask.WhenAll(tasks);
        }

        private async UniTask SafeCall(Func<bool, string, UniTask> fn, bool useUI, string sceneName)
        {
            try { await fn(useUI, sceneName); }
            
            catch (Exception ex) { DebugLogger.LogError(ex); }
        }

        private async UniTask UnloadSceneAsync() 
        { 
            var scenes = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;
            
            var sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i > 0; i--) 
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;
                
                var sceneName = sceneAt.name;
                var bootSceneName = BuildSettingsUtil.GetFirstBuildSceneName();
                if (sceneName.Equals(activeScene) || sceneName == bootSceneName) continue;
                if (_handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName)) continue;
                
                scenes.Add(sceneName);
            }
            
            var operationGroup = new AsyncOperationGroup(scenes.Count);
            
            foreach (var scene in scenes) 
            { 
                var operation = SceneManager.UnloadSceneAsync(scene);
                
                if (operation == null) continue;
                
                operationGroup.Operations.Add(operation);

                OnSceneUnloaded?.Invoke(scene);
            }
            
            foreach (var handle in _handleGroup.Handles) 
            {
                if (handle.IsValid()) 
                {
                    Addressables.UnloadSceneAsync(handle);
                }
            }
            
            _handleGroup.Handles.Clear();
    
            while (!operationGroup.IsDone) 
            {
                await UniTask.Delay(100); 
            }
            
            await Resources.UnloadUnusedAssets();
        }
        
        private static float CombinedProgress(AsyncOperationGroup op, AsyncOperationHandleGroup handle)
        {
            var nOp = op.Operations.Count;
            var nHd = handle.Handles.Count;

            if (nOp == 0 && nHd == 0) return 0f;
            if (nOp == 0) return handle.Progress;
            if (nHd == 0) return op.Progress;

            return (op.Progress * nOp + handle.Progress * nHd) / (nOp + nHd);
        }
    }
    
    public readonly struct AsyncOperationHandleGroup 
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;
        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity) 
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }

        public async UniTask ActivateResultsAsync()
        {
            foreach (var handle in Handles)
            {
                if(!handle.IsValid()) continue;
                
                await handle.Result.ActivateAsync();

                await UniTask.WaitUntil(()=> handle.IsDone);
            }
        }
    }
    
    public readonly struct AsyncOperationGroup 
    { 
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity) 
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }
}