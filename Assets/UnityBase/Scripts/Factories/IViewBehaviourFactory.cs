namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourFactory
    {
        public IViewAnimBehaviour<TAnim> CreateViewAnim<TAnim>(IViewAnimUI<TAnim> viewUI) where TAnim : class, IViewAnimation;
        public IViewModelBehaviour<TModel, TData> CreateViewModel<TModel, TData>(IViewModelUI<TModel, TData> viewUI) where TModel : IViewModel<TData> where TData : struct;
        public T GetViewUI<T>() where T : class, IViewUI;
        public bool TryGetViewUI<T>(out T viewUI) where T : class, IViewUI;
    }
}