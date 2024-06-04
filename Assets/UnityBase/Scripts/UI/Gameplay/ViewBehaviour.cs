using UnityBase.Extensions;
using UnityBase.UI.ButtonCore;


public class ViewBehaviour<TAnim> : IViewBehaviour<TAnim> where TAnim : IViewAnimation
{
    public TAnim ViewAnimation { get; }

    public ViewBehaviour(TAnim viewAnimation)
    {
        ViewAnimation = viewAnimation;
    }
    
    public void ConfigureAnimation(params object[] parameters) => ViewAnimation.ConfigureMethod("Configure", parameters);
    

    public virtual void Dispose() => ViewAnimation?.Dispose();
}
public class ViewBehaviour<TAnim, TModel, TData> : ViewBehaviour<TAnim>, IViewBehaviour<TAnim, TModel, TData> where TModel : IViewModel<TData> 
                                                                                                              where TAnim : IViewAnimation 
                                                                                                              where TData : struct
{
    public TModel ViewModel { get; }

    public ViewBehaviour(TAnim viewAnimation, TModel viewModel) : base(viewAnimation)
    {
        ViewModel = viewModel;
    }

    public void ConfigureModel(params object[] parameters) => ViewModel.ConfigureMethod("Configure", parameters);

    public override void Dispose()
    {
        base.Dispose();
        
        ViewModel?.Dispose();
    }
}