using UnityBase.UI.ButtonCore;

public interface IViewBehaviour
{
    public void Dispose();
}
public interface IViewBehaviour<out TAnim> : IViewBehaviour
{
    public TAnim ViewAnimation { get; }
    public void ConfigureAnimation(params object[] parameters);
}

public interface IViewBehaviour<out TAnim, out TModel, out TData> : IViewBehaviour<TAnim> where TModel : IViewModel<TData> where TAnim : IViewAnimation where TData : struct
{
    public TModel ViewModel { get; }

    public void ConfigureModel(params object[] parameters);
}