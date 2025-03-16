using System;
using System.Collections.Generic;
using UnityBase.BootService;
using UnityBase.Manager;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class MenuBootstarpper : IInitializable, IDisposable
    {
        [Inject]
        private readonly IEnumerable<IMenuBootService> _menuBootServices;
        
        public MenuBootstarpper(IObjectResolver objectResolver) => UpdateObjectResolvers(objectResolver);

        private static void UpdateObjectResolvers(IObjectResolver objectResolver)
        {
            var resolverContainer = objectResolver.Resolve<IObjectResolverContainer>();
            
            resolverContainer.Update(objectResolver);
        }

        public void Initialize()
        {
            foreach (var gameplayBootService in _menuBootServices)
            {
                gameplayBootService?.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var gameplayBootService in _menuBootServices)
            {
                gameplayBootService?.Dispose();
            }
        }
    }
}