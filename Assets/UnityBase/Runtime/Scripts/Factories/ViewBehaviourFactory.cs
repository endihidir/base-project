using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using UnityEngine;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourFactory : IViewBehaviourFactory
    {
        private readonly IDictionary<Type, IViewBehaviourGroup> _viewBehaviourGroup;
        
        private IObjectResolver _objectResolver;

        public ViewBehaviourFactory(IObjectResolver objectObjectResolver)
        {
            _objectResolver = objectObjectResolver;
            
            _viewBehaviourGroup = new Dictionary<Type, IViewBehaviourGroup>();
        }

        public TView CreateView<TView>(Component component) where TView : class, IView
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_objectResolver);
                
                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
           
            return viewBehaviourGroup.CreateView<TView>();
        }

        public TModel CreateModel<TModel>(Component component) where TModel : class, IModel
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_objectResolver);
                
                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
           
            return viewBehaviourGroup.CreateModel<TModel>();
        }

        public TAnim CreateAnimation<TAnim>(Component component) where TAnim : class, IAnimation
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_objectResolver);

                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
            
            return viewBehaviourGroup.CreateAnimation<TAnim>();
        }

        public TAct CreateAction<TAct>(Component component) where TAct : class, IAction
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_objectResolver);

                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
            
            return viewBehaviourGroup.CreateAction<TAct>();
        }

        public TView CreateLocalView<TView>() where TView : class, IView => ClassExtensions.CreateInstance<TView>(_objectResolver);
        public TModel CreateLocalModel<TModel>() where TModel : class, IModel => ClassExtensions.CreateInstance<TModel>(_objectResolver);
        public TAnim CreateViewLocalAnimation<TAnim>() where TAnim : class, IAnimation => ClassExtensions.CreateInstance<TAnim>(_objectResolver);
        public TAct CreateViewLocalAction<TAct>() where TAct : class, IAction => ClassExtensions.CreateInstance<TAct>(_objectResolver);

        public bool TryGetView<TViewUI, TView>(out TView view) where TViewUI : Component where TView : class, IView
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetView(out view);
            }

            view = null;
            return false;
        }

        public bool TryGetModel<TViewUI, TViewModel>(out TViewModel model) where TViewUI : Component where TViewModel : class, IModel
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetModel(out model);
            }

            model = null;
            return false;
        }

        public bool TryGetViewAnimation<TViewUI, TViewAnim>(out TViewAnim animation) where TViewUI : Component where TViewAnim : class, IAnimation
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetAnimation(out animation);
            }

            animation = null;
            return false;
        }

        public bool TryGetViewAction<TViewUI, TAct>(out TAct action) where TViewUI : Component where TAct : class, IAction
        {
            var viewKey = typeof(TViewUI);
            
            if (_viewBehaviourGroup.TryGetValue(viewKey, out var viewBehaviourGroup))
            {
                return viewBehaviourGroup.TryGetAction(out action);
            }

            action = null;
            return false;
        }
    }
}