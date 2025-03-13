
namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourGroup
    {
        public TAnim CreateAnimation<TAnim>() where TAnim : class, IViewAnimation;
        public TModel CreateModel<TModel>() where TModel : class, IViewModel;
        public bool TryGetAnimation<TAnim>(out TAnim animation) where TAnim : class, IViewAnimation;
        public bool TryGetModel<TModel>(out TModel model) where TModel : class, IViewModel;
    }
}