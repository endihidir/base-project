using UnityBase.Manager;

namespace UnityBase.UI.ButtonCore
{
    public interface IUIBehaviourFactory
    {
        public IViewBehaviour<TAnim> CreateViewBehaviour<TAnim>(ICurrencyView<TAnim> viewUI) where TAnim : IViewAnimation;
        public IViewBehaviour<TAnim,TModel, TData> CreateViewBehaviour<TAnim, TModel, TData>(ICurrencyView<TAnim,TModel, TData> viewUI) where TAnim : IViewAnimation
                                                                                                                                        where TModel : IViewModel<TData> 
                                                                                                                                        where TData : struct;
        public IButtonBehaviour<TAct, TAnim> CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction 
                                                                                                    where TAnim : IButtonAnimation;
    }
}