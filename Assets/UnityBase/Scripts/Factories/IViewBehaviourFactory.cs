namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourFactory
    {
        public TAnim CreateViewAnimation<TAnim>(IViewUI viewUI) where TAnim : class, IViewAnimation;
        public TModel CreateViewModel<TModel>(IViewUI viewUI) where TModel : class, IViewModel;
        public bool TryGetViewAnimation<TViewUI, TViewAnim>(out TViewAnim viewAnimation) where TViewUI : IViewUI where TViewAnim : class, IViewAnimation;
        public bool TryGetModel<TViewUI, TViewModel>(out TViewModel viewModel) where TViewUI : IViewUI where TViewModel : class, IViewModel;
    }
}