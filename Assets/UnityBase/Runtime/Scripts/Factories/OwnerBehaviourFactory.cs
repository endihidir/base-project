using System.Collections.Generic;
using UnityBase.Extensions;
using UnityEngine;
using VContainer;

namespace UnityBase.Runtime.Behaviours
{
    public interface IOwnerBehaviourFactory
    {
        IOwnerContext RegisterAndGetContext(Component owner);
        IOwnerContext RegisterAndGetContext(int ownerID);
        bool Release(Component owner);
        bool Release(int ownerID);

        TView CreateLocalView<TView>() where TView : class, IView;
        TModel CreateLocalModel<TModel>() where TModel : class, IModel;
        TAnim CreateLocalAnimation<TAnim>() where TAnim : class, IAnimation;
        TAct CreateLocalAction<TAct>() where TAct : class, IAction;
        TCtrl CreateLocalController<TCtrl>() where TCtrl : class, IController;
    }

    public interface IOwnerContext
    {
        TView CreateView<TView>() where TView : class, IView;
        TModel CreateModel<TModel>() where TModel : class, IModel;
        TAnim CreateAnimation<TAnim>() where TAnim : class, IAnimation;
        TAct CreateAction<TAct>() where TAct : class, IAction;
        TCtrl CreateController<TCtrl>() where TCtrl : class, IController;

        bool TryGetView<TView>(out TView view) where TView : class, IView;
        bool TryGetModel<TModel>(out TModel model) where TModel : class, IModel;
        bool TryGetAnimation<TAnim>(out TAnim animation) where TAnim : class, IAnimation;
        bool TryGetAction<TAct>(out TAct action) where TAct : class, IAction;
        bool TryGetController<TCtrl>(out TCtrl controller) where TCtrl : class, IController;
    }

    public class OwnerBehaviourFactory : IOwnerBehaviourFactory
    {
        private readonly Dictionary<int, IOwnerBehaviourGroup> _groups = new();
        private readonly IObjectResolver _resolver;

        public OwnerBehaviourFactory(IObjectResolver resolver) => _resolver = resolver;

        public IOwnerContext RegisterAndGetContext(Component owner) => !owner ? null : RegisterAndGetContext(owner.GetInstanceID());

        public IOwnerContext RegisterAndGetContext(int ownerID)
        {
            if (!_groups.TryGetValue(ownerID, out var group))
            {
                group = new OwnerBehaviourGroup(_resolver);
                _groups[ownerID] = group;
            }
            
            var ctx = new OwnerContext(group);

            return ctx;
        }

        public bool Release(Component owner)
        {
            if (!owner) return false;
            return Release(owner.GetInstanceID());
        }

        public bool Release(int ownerID)
        {
            if (_groups.TryGetValue(ownerID, out var group))
            {
                group?.Dispose();
            }
            
            return _groups.Remove(ownerID);
        }

        public TView CreateLocalView<TView>() where TView : class, IView => ClassExtensions.CreateInstance<TView>(_resolver);
        public TModel CreateLocalModel<TModel>() where TModel : class, IModel => ClassExtensions.CreateInstance<TModel>(_resolver);
        public TAnim CreateLocalAnimation<TAnim>() where TAnim : class, IAnimation => ClassExtensions.CreateInstance<TAnim>(_resolver);
        public TAct CreateLocalAction<TAct>() where TAct : class, IAction => ClassExtensions.CreateInstance<TAct>(_resolver);
        public TCtrl CreateLocalController<TCtrl>() where TCtrl : class, IController => ClassExtensions.CreateInstance<TCtrl>(_resolver);

        private sealed class OwnerContext : IOwnerContext
        {
            private readonly IOwnerBehaviourGroup _group;

            public OwnerContext(IOwnerBehaviourGroup group) => _group = group;

            public TView CreateView<TView>() where TView : class, IView => _group.CreateView<TView>();
            public TModel CreateModel<TModel>() where TModel : class, IModel => _group.CreateModel<TModel>();
            public TAnim CreateAnimation<TAnim>() where TAnim : class, IAnimation => _group.CreateAnimation<TAnim>();
            public TAct CreateAction<TAct>() where TAct : class, IAction => _group.CreateAction<TAct>();
            public TCtrl CreateController<TCtrl>() where TCtrl : class, IController => _group.CreateController<TCtrl>();

            public bool TryGetView<TView>(out TView view) where TView : class, IView => _group.TryGetView(out view);
            public bool TryGetModel<TModel>(out TModel model) where TModel : class, IModel => _group.TryGetModel(out model);
            public bool TryGetAnimation<TAnim>(out TAnim animation) where TAnim : class, IAnimation => _group.TryGetAnimation(out animation);
            public bool TryGetAction<TAct>(out TAct action) where TAct : class, IAction => _group.TryGetAction(out action);
            public bool TryGetController<TCtrl>(out TCtrl controller) where TCtrl : class, IController => _group.TryGetController(out controller);
        }
    }
}
