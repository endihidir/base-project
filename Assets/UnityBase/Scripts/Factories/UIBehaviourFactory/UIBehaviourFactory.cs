using UnityBase.Extensions;
using UnityBase.Manager;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public class UIBehaviourFactory : IUIBehaviourFactory
    {
        private readonly IObjectResolver _container;

        public UIBehaviourFactory(IObjectResolver container)
        {
            _container = container;
        }

        public IViewBehaviour<TAnim> CreateViewBehaviour<TAnim>(ICurrencyView<TAnim> viewUI) where TAnim : IViewAnimation
        {
            var viewAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, viewUI);
            
            return new ViewBehaviour<TAnim>(viewAnimation);
        }

        public IViewBehaviour<TAnim, TModel, TData> CreateViewBehaviour<TAnim, TModel, TData>(ICurrencyView<TAnim,TModel, TData> viewUI) where TAnim : IViewAnimation
                                                                                                                                         where TModel : IViewModel<TData> 
                                                                                                                                         where TData : struct
        {
            var viewModel = ReflectionExtensions.CreateInstance<TModel>(_container, viewUI);
            
            var viewAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, viewUI);
            
            return new ViewBehaviour<TAnim, TModel, TData>(viewAnimation ,viewModel);
        }
        
        public IButtonBehaviour<TAct, TAnim> CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction where TAnim : IButtonAnimation
        {
            var buttonAction = ReflectionExtensions.CreateInstance<TAct>(_container, buttonUI);
            
            var buttonAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, buttonUI);
            
            return new ButtonBehaviour<TAct, TAnim>(buttonAction, buttonAnimation);
        }
    }
}