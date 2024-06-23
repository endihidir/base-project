using System;
using Cysharp.Threading.Tasks;
using UnityBase.SceneManagement;

namespace UnityBase.Service
{
    public interface ISceneManager
    {
        public event Action<SceneType> OnSceneLoaded;
        public LoadingProgress LoadingProgress { get; }
        public UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, float delayMultiplier = 10f);
    }
}