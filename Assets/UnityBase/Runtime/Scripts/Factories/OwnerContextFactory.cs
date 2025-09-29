using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.Runtime.Factories
{
    public interface IOwnerContextFactory
    {
        IOwnerContext GetContext(Component owner);
        IOwnerContext GetContext(GameObject owner);
        IOwnerContext GetContext(int ownerID);
        bool Release(Component owner);
        bool Release(GameObject owner);
        bool Release(int ownerID);
    }

    public interface IOwnerContext
    {
        TModel ResolveModel<TModel>() where TModel : class, IModel;
        TView ResolveView<TView>() where TView : class, IView;
        TAnim ResolveAnimation<TAnim>() where TAnim : class, IAnimation;
        TAct ResolveAction<TAct>() where TAct : class, IAction;
        TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter;
        bool TryGetPresenter<TPresenter>(out TPresenter presenter) where TPresenter : class, IPresenter;
        void Dispose();
    }

    public class OwnerContextFactory : IOwnerContextFactory
    {
        private readonly Dictionary<int, IOwnerContext> _contexts = new();
        private readonly IAmbientResolverProvider _ambientResolverProvider;
        private readonly IModelFactory _modelFactory;
        private readonly IActionFactory _actionFactory;

        public OwnerContextFactory(IAmbientResolverProvider ambientResolverProvider, IModelFactory modelFactory, IActionFactory actionFactory)
        {
            _ambientResolverProvider = ambientResolverProvider;
            _modelFactory = modelFactory;
            _actionFactory = actionFactory;
        }

        public IOwnerContext GetContext(Component owner)
        {
            if (!owner)
            {
                DebugLogger.LogError("[ContextFactory] GetContext(Component): owner is null.");
                return null;
            }
            
            return GetContext(owner.GetInstanceID());
        }

        public IOwnerContext GetContext(GameObject owner)
        {
            if (!owner)
            {
                DebugLogger.LogError("[ContextFactory] GetContext(GameObject): owner is null.");
                return null;
            }
            
            return GetContext(owner.GetInstanceID());
        }

        public IOwnerContext GetContext(int ownerID)
        {
            if (!_contexts.TryGetValue(ownerID, out var context))
            {
                context = new OwnerContext(_ambientResolverProvider, _modelFactory, _actionFactory);
                _contexts[ownerID] = context;
            }

            return context;
        }

        public bool Release(Component owner)
        {
            if (!owner)
            {
                DebugLogger.LogError("[ContextFactory] Release(Component): owner is null.");
                return false;
            }
            
            return Release(owner.GetInstanceID());
        }

        public bool Release(GameObject owner)
        {
            if (!owner)
            {
                DebugLogger.LogError("[ContextFactory] Release(GameObject): owner is null.");
                return false;
            }
            
            return Release(owner.GetInstanceID());
        }

        public bool Release(int ownerID)
        {
            if (_contexts.TryGetValue(ownerID, out var ownerContext))
            {
                ownerContext?.Dispose();
            }
            
            return _contexts.Remove(ownerID);
        }

        private sealed class OwnerContext : IOwnerContext
        {
            private readonly IOwnerContextGroup _ownerContextGroup;
            private readonly IModelFactory _modelFactory;
            private readonly IActionFactory _actionFactory;

            public OwnerContext(IAmbientResolverProvider ambientResolverProvider, IModelFactory modelFactory, IActionFactory actionFactory)
            {
                _ownerContextGroup = new OwnerContextGroup(ambientResolverProvider);
                _modelFactory = modelFactory;
                _actionFactory = actionFactory;
            }

            public TModel ResolveModel<TModel>() where TModel : class, IModel => _modelFactory.Resolve<TModel>();
            public TAct ResolveAction<TAct>() where TAct : class, IAction => _actionFactory.Resolve<TAct>();
            public TView ResolveView<TView>() where TView : class, IView => _ownerContextGroup.ResolveView<TView>();
            public TAnim ResolveAnimation<TAnim>() where TAnim : class, IAnimation => _ownerContextGroup.ResolveAnimation<TAnim>();
            public TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter => _ownerContextGroup.ResolvePresenter<TPresenter>();
            public bool TryGetPresenter<TPresenter>(out TPresenter presenter) where TPresenter : class, IPresenter => _ownerContextGroup.TryGetPresenter(out presenter);
            public void Dispose() => _ownerContextGroup?.Dispose();
        }
    }
}