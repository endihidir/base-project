﻿using System;
using Cysharp.Threading.Tasks;

namespace UnityBase.Service
{
    public interface ISceneDataService
    {
        public event Action<float> OnLoadUpdate;
        public UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, float progressMultiplier = 10f);
    }
}