
namespace UnityBase.UI.ViewCore
{ 
    public interface IViewBehaviour
    {
        public void Dispose();
    }

    public interface IViewBehaviour<out TAnim> : IViewBehaviour where TAnim : IViewAnimation 
    {
        public TAnim ViewAnimation { get; }
        public IViewBehaviour<TAnim> ConfigureAnimation(params object[] parameters);
    }

    public interface IViewBehaviour<out TModel, in TData> : IViewBehaviour where TModel : IViewModel<TData> where TData : struct 
    {
        public TModel ViewModel { get; }
        public IViewBehaviour<TModel, TData> ConfigureModel(params object[] parameters);
        public void UpdateValue(TData data);
    }
}
