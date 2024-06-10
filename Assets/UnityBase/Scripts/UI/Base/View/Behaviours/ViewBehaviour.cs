using UnityBase.Extensions;

namespace UnityBase.UI.ViewCore
{
    public class ViewAnimBehaviour<TAnim> : IViewBehaviour<TAnim>  where TAnim : IViewAnimation 
    {
        public TAnim ViewAnimation { get; }

        public ViewAnimBehaviour(TAnim viewAnimation) => ViewAnimation = viewAnimation;

        public IViewBehaviour<TAnim> ConfigureAnimation(params object[] parameters)
        {
            ViewAnimation.ConfigureMethod("Configure", parameters);
            return this;
        }

        public void Dispose() => ViewAnimation?.Dispose();
    }

    public class ViewModelBehaviour<TModel, TData> : IViewBehaviour<TModel, TData> where TModel : IViewModel<TData> where TData : struct
    {
        public TModel ViewModel { get; }

        public ViewModelBehaviour(TModel viewModel) => ViewModel = viewModel;

        public IViewBehaviour<TModel, TData> ConfigureModel(params object[] parameters)
        {
            ViewModel?.ConfigureMethod("Configure", parameters);
            return this;
        }

        public void UpdateValue(TData data) => ViewModel?.ChangeValue(data);
        public void Dispose() => ViewModel?.Dispose();
    }
}