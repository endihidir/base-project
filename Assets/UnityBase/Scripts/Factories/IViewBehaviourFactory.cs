namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourFactory
    {
        public IViewBehaviour<TAnim> CreateViewAnimationBehaviour<TAnim>(IViewUI viewUI) where TAnim : class, IViewAnimation;
        public IViewBehaviour<TModel, TData> CreateViewModelBehaviour<TModel, TData>(IViewUI viewUI) where TModel : IViewModel<TData> where TData : struct;
        public bool TryGetAnimationBehaviour<TViewUI>(out IViewBehaviour<IViewAnimation> viewBehaviour) where TViewUI : IViewUI;
        public bool TryGetModelBehaviour<TViewUI>(out IViewBehaviour viewBehaviour) where TViewUI : IViewUI;
    }
}