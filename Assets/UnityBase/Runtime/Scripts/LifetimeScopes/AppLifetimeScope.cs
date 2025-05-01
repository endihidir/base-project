using UnityBase.BlackboardCore;
using UnityBase.GameDataHolder;
using UnityBase.Manager;
using UnityBase.Presenter;
using UnityBase.SceneManagement;
using UnityBase.StateMachineCore;
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
            builder.Register<ObjectResolverContainer>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CurrencyManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<GlobalBlackboard>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ViewBehaviourFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SaveManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<GlobalStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StateMachineFactory>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }   
}