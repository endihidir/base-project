using UnityBase.Manager;
using UnityBase.Presenter;
using VContainer;
using VContainer.Unity;

namespace UnityBase.BaseLifetimeScope
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameplayBootstarpper>();
            builder.Register<GameplayManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CinemachineManager>(Lifetime.Singleton).AsImplementedInterfaces();

            //builder.RegisterComponentInHierarchy<CurrencyUI>().AsImplementedInterfaces();
        }
    }
}
