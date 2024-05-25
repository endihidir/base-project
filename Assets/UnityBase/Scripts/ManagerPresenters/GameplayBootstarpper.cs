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
        
        public GameplayBootstarpper(IObjectResolver objectResolver, ICoinView coinView)
        {
            UpdateGameplayServices(objectResolver, coinView);
        }

        private static void UpdateGameplayServices(IObjectResolver objectResolver, ICoinView coinView)
        {
            var poolManager = objectResolver.Resolve<IPoolManagementService>() as PoolManager;
            poolManager?.UpdateAllResolvers(objectResolver);

            var currencyManager = objectResolver.Resolve<ICurrencyViewService>() as CurrencyManager;
            currencyManager?.SetCoinViewData(coinView);
        }
        
        public void Initialize() => _gameplayBootServices.ForEach(x => x.Initialize());
        public void Dispose() => _gameplayBootServices.ForEach(x => x.Dispose());
    }
}