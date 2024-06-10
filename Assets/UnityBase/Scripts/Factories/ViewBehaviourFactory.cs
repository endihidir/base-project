using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourFactory : IViewBehaviourFactory
    {
        private readonly IDictionary<Type, IViewAnimationGroup> _viewAnimations;

        private readonly IDictionary<Type, IViewBehaviour> _viewModels;
        
        private readonly IObjectResolver _container;

        public ViewBehaviourFactory(IObjectResolver container)
        {
            _container = container;

            _viewAnimations = new Dictionary<Type, IViewAnimationGroup>();
        }

        public IViewBehaviour<TAnim> CreateViewAnimationBehaviour<TAnim>(IViewUI viewUI) where TAnim : class, IViewAnimation
        {
            var viewAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, viewUI);
            
            var viewBehaviour = new ViewAnimBehaviour<TAnim>(viewAnimation);
            
            AddViewAnimation(viewUI.Key, viewBehaviour);

            return viewBehaviour;
        }

        public IViewBehaviour<TModel, TData> CreateViewModelBehaviour<TModel, TData>(IViewUI viewUI) where TModel : IViewModel<TData> where TData : struct
        {
            var viewModel = ReflectionExtensions.CreateInstance<TModel>(_container, viewUI);

            var viewBehaviour = new ViewModelBehaviour<TModel, TData>(viewModel);
            
            _viewModels[viewUI.Key] = viewBehaviour;

            return viewBehaviour;
        }

        public bool TryGetAnimationBehaviour<TViewUI>(out IViewBehaviour<IViewAnimation> viewBehaviour) where TViewUI : IViewUI
        {
            viewBehaviour = null;
            
            var key = typeof(TViewUI);
            
            return _viewAnimations.TryGetValue(key, out var viewAnimationGroup) && viewAnimationGroup.TryGetAnimationBehaviour(out viewBehaviour);
        }

        public bool TryGetModelBehaviour<TViewUI>(out IViewBehaviour viewBehaviour) where TViewUI : IViewUI
        {
            viewBehaviour = null;
            
            var key = typeof(TViewUI);

            return _viewModels.TryGetValue(key, out viewBehaviour);
        }
        
        private void AddViewAnimation(Type key, IViewBehaviour<IViewAnimation> viewBehaviour)
        {
            if (!_viewAnimations.TryGetValue(key, out var viewAnimationGroup))
            {
                viewAnimationGroup = new ViewAnimationGroup();
                
                _viewAnimations[key] = viewAnimationGroup;
            }
            
            _viewAnimations[key].AddAnimationBehaviour(viewBehaviour);
        }
    }
}