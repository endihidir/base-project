using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityBase.Manager;
using UnityBase.Service;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class GameplayBootstarpper : IInitializable, IDisposable
    {
        [Inject]
        private readonly IEnumerable<IGameplayBootService> _gameplayBootServices;
        
        public GameplayBootstarpper(IObjectResolver objectResolver) => UpdateObjectResolvers(objectResolver);

        private static void UpdateObjectResolvers(IObjectResolver objectResolver)
        {
            var resolverContainer = objectResolver.Resolve<IObjectResolverContainer>();
            
            resolverContainer.Update(objectResolver);

            var resolverUpdaters = objectResolver.Resolve<IEnumerable<IResolverUpdater>>();
            
            foreach (var resolverUpdater in resolverUpdaters)
            {
                resolverUpdater.UpdateResolver(objectResolver);
            }
        }

        public void Initialize() => _gameplayBootServices.ForEach(x => x.Initialize());
        public void Dispose() => _gameplayBootServices.ForEach(x => x.Dispose());
    }
    
    public interface IResolverUpdater
    {
        public void UpdateResolver(IObjectResolver objectResolver);
    }
}