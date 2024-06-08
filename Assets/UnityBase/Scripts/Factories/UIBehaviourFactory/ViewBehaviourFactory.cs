using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using UnityBase.Manager;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public class ViewBehaviourFactory : IViewBehaviourFactory
    {
        private readonly IDictionary<Type, IViewUI> _viewAnimBehaviours;
        
        private readonly IObjectResolver _container;

        public ViewBehaviourFactory(IObjectResolver container)
        {
            _container = container;

            _viewAnimBehaviours = new Dictionary<Type, IViewUI>();
        }

        public IViewAnimBehaviour<TAnim> CreateViewAnim<TAnim>(IViewAnimUI<TAnim> viewUI) where TAnim : class, IViewAnimation
        {
            _viewAnimBehaviours[viewUI.Key] = viewUI;
            
            var viewAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, viewUI);
            
            var viewBehaviour = new ViewAnimBehaviour<TAnim>(viewAnimation);

            return viewBehaviour;
        }

        public IViewModelBehaviour<TModel, TData> CreateViewModel<TModel, TData>(IViewModelUI<TModel, TData> viewUI) where TModel : IViewModel<TData> where TData : struct
        {
            _viewAnimBehaviours[viewUI.Key] = viewUI;
            
            var viewModel = ReflectionExtensions.CreateInstance<TModel>(_container, viewUI);

            var viewBehaviour = new ViewModelBehaviour<TModel, TData>(viewModel);

            return viewBehaviour;
        }

        public T GetViewUI<T>() where T : class, IViewUI
        {
            var key = typeof(T);
            
            if (_viewAnimBehaviours.TryGetValue(key, out var viewUI))
            {
                return viewUI as T;
            }
            
            return null;
        }
    }
    
    public interface IViewBehaviourFactory
    {
        public IViewAnimBehaviour<TAnim> CreateViewAnim<TAnim>(IViewAnimUI<TAnim> viewUI) where TAnim : class, IViewAnimation;

        public IViewModelBehaviour<TModel, TData> CreateViewModel<TModel, TData>(IViewModelUI<TModel, TData> viewUI) where TModel : IViewModel<TData> where TData : struct;
        public T GetViewUI<T>() where T : class, IViewUI;
    }
}