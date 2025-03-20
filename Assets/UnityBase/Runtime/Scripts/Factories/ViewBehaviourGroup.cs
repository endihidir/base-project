using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourGroup : IViewBehaviourGroup
    {
        private readonly IDictionary<Type, IAnimation> _animations = new Dictionary<Type, IAnimation>();
        
        private readonly IDictionary<Type, IModel> _models = new Dictionary<Type, IModel>();
        
        private readonly IDictionary<Type, IView> _views = new Dictionary<Type, IView>();
        
        private readonly IDictionary<Type, IAction> _actions = new Dictionary<Type, IAction>();

        private readonly IObjectResolver _resolver;
        
        public ViewBehaviourGroup(IObjectResolver resolver) => _resolver = resolver;

        public TAnim CreateAnimation<TAnim>() where TAnim : class, IAnimation
        {
            var key = typeof(TAnim);

            if (!_animations.TryGetValue(key, out var viewAnimation))
            {
                viewAnimation = ClassExtensions.CreateInstance<TAnim>(_resolver);
                
                _animations[key] = viewAnimation;
            }

            return viewAnimation as TAnim;
        }

        public TModel CreateModel<TModel>() where TModel : class, IModel
        {
            var key = typeof(TModel);

            if (!_models.TryGetValue(key, out var viewModel))
            {
                viewModel = ClassExtensions.CreateInstance<TModel>(_resolver);
                
                _models[key] = viewModel;
            }

            return viewModel as TModel;
        }

        public TView CreateView<TView>() where TView : class, IView
        {
            var key = typeof(TView);

            if (!_views.TryGetValue(key, out var view))
            {
                view = ClassExtensions.CreateInstance<TView>(_resolver);
                
                _views[key] = view;
            }

            return view as TView;
        }

        public TAct CreateAction<TAct>() where TAct : class, IAction
        {
            var key = typeof(TAct);

            if (!_actions.TryGetValue(key, out var action))
            {
                action = ClassExtensions.CreateInstance<TAct>(_resolver);
                
                _actions[key] = action;
            }

            return action as TAct;
        }

        public bool TryGetAnimation<TAnim>(out TAnim value) where TAnim : class, IAnimation
        {
            var key = typeof(TAnim);
            
            if (_animations.TryGetValue(key, out var viewAnimation))
            {
                value = viewAnimation as TAnim;
                return true;
            }

            value = null;
            
            return false;
        }

        public bool TryGetModel<TModel>(out TModel value) where TModel : class, IModel
        {
            var key = typeof(TModel);
            
            if (_models.TryGetValue(key, out var model))
            {
                value = model as TModel;
                return true;
            }

            value = null;
            
            return false;
        }

        public bool TryGetView<TView>(out TView value) where TView : class, IView
        {
            var key = typeof(TView);
            
            if (_views.TryGetValue(key, out var view))
            {
                value = view as TView;
                return true;
            }

            value = null;
            
            return false;
        }

        public bool TryGetAction<TAct>(out TAct value) where TAct : class, IAction
        {
            var key = typeof(TAct);
            
            if (_actions.TryGetValue(key, out var action))
            {
                value = action as TAct;
                return true;
            }

            value = null;
            return false;
        }
    }
}