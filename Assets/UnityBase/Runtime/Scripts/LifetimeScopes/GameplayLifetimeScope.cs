using UnityBase.Manager;
using UnityBase.Presenters;
using VContainer;
using VContainer.Unity;

namespace UnityBase.BaseLifetimeScope
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameplayPresenter>();
            
            builder.Register<GameplayManager>(Lifetime.Singleton).As<IGameplayManager>();
            
            builder.Register<CinemachineManager>(Lifetime.Singleton).As<ICinemachineManager>();
        }
    }
}
