using UnityEngine;

namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourFactory
    {
        public TModel CreateViewModel<TModel>(Component component) where TModel : class, IViewModel;
        public TAnim CreateViewAnimation<TAnim>(Component component) where TAnim : class, IViewAnimation;
        public TModel CreateViewLocalModel<TModel>() where TModel : class, IViewModel;
        public TAnim CreateViewLocalAnimation<TAnim>() where TAnim : class, IViewAnimation;
        public bool TryGetModel<TViewUI, TViewModel>(out TViewModel viewModel) where TViewUI : Component where TViewModel : class, IViewModel;
        public bool TryGetViewAnimation<TViewUI, TViewAnim>(out TViewAnim viewAnimation) where TViewUI : Component where TViewAnim : class, IViewAnimation;
    }
}