using UnityBase.UI.ButtonCore;

namespace UnityBase.UI.ViewCore
{ 
    public interface IViewBehaviour
    {
        public void Dispose();
    }

    public interface IViewAnimBehaviour<out TAnim> : IViewBehaviour where TAnim : IViewAnimation 
    {
        public TAnim ViewAnimation { get; }
        public IViewAnimBehaviour<TAnim> ConfigureAnimation(params object[] parameters);
    }

    public interface IViewModelBehaviour<out TModel, TData> : IViewBehaviour where TModel : IViewModel<TData> where TData : struct 
    {
        public TModel ViewModel { get; }
        public IViewModelBehaviour<TModel, TData> ConfigureModel(params object[] parameters);
    }
}
