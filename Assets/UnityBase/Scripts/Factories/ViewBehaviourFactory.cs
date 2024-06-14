using System;
using System.Collections.Generic;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourFactory : IViewBehaviourFactory
    {
        private readonly IDictionary<Type, IViewBehaviourGroup> _viewBehaviourGroup;
        
        private readonly IObjectResolver _container;

        public ViewBehaviourFactory(IObjectResolver container)
        {
            _container = container;
            _viewBehaviourGroup = new Dictionary<Type, IViewBehaviourGroup>();
        }

        public TAnim CreateViewAnimation<TAnim>(IViewUI viewUI) where TAnim : class, IViewAnimation
        {
            var key = viewUI.GetType();

            if (!_viewBehaviourGroup.TryGetValue(key, out var viewAnimationGroup))
            {
                viewAnimationGroup = new ViewBehaviourGroup(_container);

                _viewBehaviourGroup[key] = viewAnimationGroup;
            }
            
            return viewAnimationGroup.CreateAnimation<TAnim>();
        }

        public TModel CreateViewModel<TModel>(IViewUI viewUI) where TModel : class, IViewModel
        {
            var key = viewUI.GetType();

            if (!_viewBehaviourGroup.TryGetValue(key, out var viewAnimationGroup))
            {
                viewAnimationGroup = new ViewBehaviourGroup(_container);
                
                _viewBehaviourGroup[key] = viewAnimationGroup;
            }
           
            return viewAnimationGroup.CreateModel<TModel>();
        }

        public bool TryGetViewAnimation<TViewUI, TViewAnim>(out TViewAnim viewAnimation) where TViewUI : IViewUI where TViewAnim : class, IViewAnimation
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetAnimation(out viewAnimation);
            }

            viewAnimation = null;
            return false;
        }

        public bool TryGetModel<TViewUI, TViewModel>(out TViewModel viewModel) where TViewUI : IViewUI where TViewModel : class, IViewModel
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetModel(out viewModel);
            }

            viewModel = null;
            return false;
        }
    }
}