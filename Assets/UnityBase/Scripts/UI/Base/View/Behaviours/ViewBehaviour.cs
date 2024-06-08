using UnityBase.Extensions;
using UnityBase.UI.ButtonCore;

namespace UnityBase.UI.ViewCore
{
    public class ViewAnimBehaviour<TAnim> : IViewAnimBehaviour<TAnim>  where TAnim : IViewAnimation 

    {
        public TAnim ViewAnimation { get; }

        public ViewAnimBehaviour(TAnim viewAnimation)
        {
            ViewAnimation = viewAnimation;
        }

        public IViewAnimBehaviour<TAnim> ConfigureAnimation(params object[] parameters)
        {
            ViewAnimation.ConfigureMethod("Configure", parameters);
            return this;
        }

        public void Dispose() => ViewAnimation?.Dispose();
    }

    public class ViewModelBehaviour<TModel, TData> : IViewModelBehaviour<TModel, TData> where TModel : IViewModel<TData> where TData : struct

    {
        public TModel ViewModel { get; }

        public ViewModelBehaviour(TModel viewModel)
        {
            ViewModel = viewModel;
        }
        
        public IViewModelBehaviour<TModel, TData> ConfigureModel(params object[] parameters)
        {
            ViewModel.ConfigureMethod("Configure", parameters);
            return this;
        }

        public void Dispose() => ViewModel?.Dispose();
    }
}