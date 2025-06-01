using System;
using System.Collections.Generic;
using UnityBase.BootService;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class GameplayBootstarpper : IInitializable, IDisposable
    {
        [Inject]
        private readonly IEnumerable<IGameplayBootService> _gameplayBootServices;
        
        public void Initialize()
        {
            foreach (var gameplayBootService in _gameplayBootServices)
            {
                gameplayBootService?.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var gameplayBootService in _gameplayBootServices)
            {
                gameplayBootService?.Dispose();
            }
        }
    }
}