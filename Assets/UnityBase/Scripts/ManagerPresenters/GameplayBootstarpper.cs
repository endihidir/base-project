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
        
        public GameplayBootstarpper(IObjectResolver objectResolver)
        {
            UpdateGameplayServices(objectResolver);
        }

        private static void UpdateGameplayServices(IObjectResolver objectResolver)
        {
            var poolManager = objectResolver.Resolve<IPoolManager>() as PoolManager;
            poolManager?.UpdateAllResolvers(objectResolver);

            /*var currencyManager = objectResolver.Resolve<ICurrencyViewService>() as CurrencyManager;
            currencyManager?.SetCoinViewData(currencyView);*/
        }
        
        public void Initialize() => _gameplayBootServices.ForEach(x => x.Initialize());
        public void Dispose() => _gameplayBootServices.ForEach(x => x.Dispose());
    }
}