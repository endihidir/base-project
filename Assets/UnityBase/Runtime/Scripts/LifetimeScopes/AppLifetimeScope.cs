using UnityBase.BlackboardCore;
using UnityBase.GameDataHolder;
using UnityBase.Manager;
using UnityBase.Presenter;
using UnityBase.SceneManagement;
using UnityBase.StateMachineCore;
using UnityBase.Runtime.Behaviours;
using UnityBase.SaveSystem;
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
            
            if (!FindObjectOfType<SaveDispatcher>())
            {
                var go = new GameObject("SaveDispatcher");
                go.AddComponent<SaveDispatcher>();
            }
            
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
            builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LevelManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<PoolManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PopUpManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialActionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialMaskManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TutorialProcessManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CurrencyManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SaveManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StateMachineManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<OwnerBehaviourFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<BlackboardRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }   
}