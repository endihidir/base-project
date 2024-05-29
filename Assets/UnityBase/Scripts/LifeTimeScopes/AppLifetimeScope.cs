using UnityBase.Command;
using UnityBase.Manager;
using UnityBase.Presenter;
using UnityBase.ManagerSO;
using UnityBase.SceneManagement;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UnityBase.BaseLifetimeScope
{
    public class AppLifetimeScope : LifetimeScope
    {
        [SerializeField] private ManagerDataHolderSO _managerDataHolderSo;

        protected override void Configure(IContainerBuilder builder)
        {
            _managerDataHolderSo.Initialize();
            
            builder.RegisterInstance(_managerDataHolderSo);

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

            builder.Register<ButtonBehaviourFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<PoolManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PopUpManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialActionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialMaskManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialProcessManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CurrencyManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TaskManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SwipeManager>(Lifetime.Singleton).AsImplementedInterfaces();
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