using UnityBase.BlackboardCore;
using UnityBase.GameDataHolder;
using UnityBase.Manager;
using UnityBase.Presenters;
using UnityBase.StateMachineCore;
using UnityBase.Runtime.Factories;
using UnityBase.SaveSystem;
using UnityBase.Service;
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
                go.transform.SetParent(transform);
            }
            
            builder.RegisterInstance(gameDataHolderSo);
            
            builder.RegisterEntryPoint<GamePresenter>();

            RegisterSingletonServices(builder);
        }
        
        private void RegisterSingletonServices(IContainerBuilder builder)
        {
            builder.Register<AmbientResolverProvider>(Lifetime.Singleton).As<IAmbientResolverProvider>().AsSelf();
            builder.Register<GameManager>(Lifetime.Singleton).As<IGameManager>();
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService>();
            builder.Register<LevelManager>(Lifetime.Singleton).As<ILevelManager>();
            
            builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolManager>();
            builder.Register<PopUpManager>(Lifetime.Singleton).As<IPopUpManager>();
            builder.Register<TutorialActionManager>(Lifetime.Singleton).As<ITutorialActionManager>();
            builder.Register<TutorialMaskManager>(Lifetime.Singleton).As<ITutorialMaskManager>();
            builder.Register<TutorialProcessManager>(Lifetime.Singleton).As<ITutorialProcessManager>();

            builder.Register<CommandManager>(Lifetime.Singleton).As<ICommandManager>();
            builder.Register<CurrencyManager>(Lifetime.Singleton).As<ICurrencyManager>();
            builder.Register<SaveManager>(Lifetime.Singleton).As<ISaveManager>();
            builder.Register<StateMachineManager>(Lifetime.Singleton).As<IStateMachineManager>();
            
            builder.Register<OwnerContextFactory>(Lifetime.Singleton).As<IOwnerContextFactory, ITickable>();
            builder.Register<ModelFactory>(Lifetime.Singleton).As<IModelFactory>();
            builder.Register<ActionFactory>(Lifetime.Singleton).As<IActionFactory>();
            
            builder.Register<BlackboardContainer>(Lifetime.Singleton).As<IBlackboardContainer>();
        }
    }   
}