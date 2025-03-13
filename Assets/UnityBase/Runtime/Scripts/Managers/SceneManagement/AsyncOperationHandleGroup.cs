using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityBase.SceneManagement
{
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
}