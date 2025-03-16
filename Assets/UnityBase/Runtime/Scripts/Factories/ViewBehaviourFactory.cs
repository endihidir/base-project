using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using UnityBase.Manager;
using UnityEngine;

namespace UnityBase.UI.ViewCore
{
    public class ViewBehaviourFactory : IViewBehaviourFactory
    {
        private readonly IDictionary<Type, IViewBehaviourGroup> _viewBehaviourGroup;
        
        private IObjectResolverContainer _resolverContainer;

        public ViewBehaviourFactory(IObjectResolverContainer resolverContainerContainer)
        {
            _resolverContainer = resolverContainerContainer;
            
            _viewBehaviourGroup = new Dictionary<Type, IViewBehaviourGroup>();
        }

        public TView CreateView<TView>(Component component) where TView : class, IView
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_resolverContainer.ObjectResolver);
                
                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
           
            return viewBehaviourGroup.CreateView<TView>();
        }

        public TModel CreateModel<TModel>(Component component) where TModel : class, IModel
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_resolverContainer.ObjectResolver);
                
                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
           
            return viewBehaviourGroup.CreateModel<TModel>();
        }

        public TAnim CreateAnimation<TAnim>(Component component) where TAnim : class, IAnimation
        {
            var key = component.GetType();
            
            if (!_viewBehaviourGroup.TryGetValue(key, out var viewBehaviourGroup))
            {
                viewBehaviourGroup = new ViewBehaviourGroup(_resolverContainer.ObjectResolver);

                _viewBehaviourGroup[key] = viewBehaviourGroup;
            }
            
            return viewBehaviourGroup.CreateAnimation<TAnim>();
        }

        public TView CreateLocalView<TView>() where TView : class, IView => ClassExtensions.CreateInstance<TView>(_resolverContainer);
        public TModel CreateLocalModel<TModel>() where TModel : class, IModel => ClassExtensions.CreateInstance<TModel>(_resolverContainer);
        public TAnim CreateViewLocalAnimation<TAnim>() where TAnim : class, IAnimation => ClassExtensions.CreateInstance<TAnim>(_resolverContainer);
        
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
    }
}