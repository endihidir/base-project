using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourGroup : IViewBehaviourGroup
    {
        private readonly IDictionary<Type, IViewAnimation> _viewAnimations = new Dictionary<Type, IViewAnimation>();
        
        private readonly IDictionary<Type, IViewModel> _viewModels = new Dictionary<Type, IViewModel>();

        private readonly IObjectResolver _container;
        
        public ViewBehaviourGroup(IObjectResolver container) => _container = container;

        public TAnim CreateAnimation<TAnim>() where TAnim : class, IViewAnimation
        {
            var key = typeof(TAnim);

            if (!_viewAnimations.TryGetValue(key, out var viewAnimation))
            {
                viewAnimation = ClassExtensions.CreateInstance<TAnim>(_container);
                
                _viewAnimations[key] = viewAnimation;
            }

            return viewAnimation as TAnim;
        }

        public TModel CreateModel<TModel>() where TModel : class, IViewModel
        {
            var key = typeof(TModel);

            if (!_viewModels.TryGetValue(key, out var viewModel))
            {
                viewModel = ClassExtensions.CreateInstance<TModel>(_container);
                
                _viewModels[key] = viewModel;
            }

            return viewModel as TModel;
        }

        public bool TryGetAnimation<TAnim>(out TAnim animation) where TAnim : class, IViewAnimation
        {
            var key = typeof(TAnim);
            
            if (_viewAnimations.TryGetValue(key, out var viewAnimation))
            {
                animation = viewAnimation as TAnim;
                return true;
            }

            animation = null;
            
            return false;
        }

        public bool TryGetModel<TModel>(out TModel model) where TModel : class, IViewModel
        {
            var key = typeof(TModel);
            
            if (_viewModels.TryGetValue(key, out var viewModel))
            {
                model = viewModel as TModel;
                return true;
            }

            model = null;
            
            return false;
        }
    }
}