using UnityBase.Command;
using UnityBase.GameDataHolder;
using UnityBase.Manager;
using UnityBase.Presenter;
using UnityBase.SceneManagement;
using UnityBase.UI.ButtonCore;
using UnityBase.UI.ViewCore;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UnityBase.BaseLifetimeScope
{
    public class AppLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameDataHolderSO gameDataHolderSo;

        protected override void Configure(IContainerBuilder builder)
        {
            gameDataHolderSo.Initialize();
            
            builder.RegisterInstance(gameDataHolderSo);

            RegisterEntryPoints(builder);

            RegisterSingletonServices(builder);

            RegisterScopedServices(builder);
            
            RegisterTransientServices(builder);
        }
        
        private void RegisterEntryPoints(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<AppBootstrapper>();
        }
        
          private void RegisterSingletonServices(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SceneGroupManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LevelManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<PoolManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PopUpManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialActionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialMaskManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialProcessManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CoinManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TaskManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SwipeManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<ViewBehaviourFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ButtonBehaviourFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterScopedServices(IContainerBuilder builder)
        {
            builder.Register<JsonDataManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<CommandRecorder>(Lifetime.Scoped).AsImplementedInterfaces();
        }
        
        private void RegisterTransientServices(IContainerBuilder builder)
        {
           
        }
    }   
}