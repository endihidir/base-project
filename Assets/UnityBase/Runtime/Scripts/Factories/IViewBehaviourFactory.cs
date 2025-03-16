using UnityEngine;
using VContainer;

namespace UnityBase.UI.ViewCore
{
    public interface IViewBehaviourFactory
    {
        public TView CreateView<TView>(Component component) where TView : class, IView;
        public TModel CreateModel<TModel>(Component component) where TModel : class, IModel;
        public TAnim CreateAnimation<TAnim>(Component component) where TAnim : class, IAnimation;
        public TView CreateLocalView<TView>() where TView : class, IView;
        public TModel CreateLocalModel<TModel>() where TModel : class, IModel;
        public TAnim CreateViewLocalAnimation<TAnim>() where TAnim : class, IAnimation;
        public bool TryGetView<TViewUI, TView>(out TView view) where TViewUI : Component where TView : class, IView;
        public bool TryGetModel<TViewUI, TModel>(out TModel model) where TViewUI : Component where TModel : class, IModel;
        public bool TryGetViewAnimation<TViewUI, TAnim>(out TAnim animation) where TViewUI : Component where TAnim : class, IAnimation;
    }
}