
namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourGroup
    {
        public TAnim CreateAnimation<TAnim>() where TAnim : class, IAnimation;
        public TModel CreateModel<TModel>() where TModel : class, IModel;
        public TView CreateView<TView>() where TView : class, IView;
        public bool TryGetAnimation<TAnim>(out TAnim value) where TAnim : class, IAnimation;
        public bool TryGetModel<TModel>(out TModel value) where TModel : class, IModel;
        public bool TryGetView<TView>(out TView value) where TView : class, IView;
    }
}