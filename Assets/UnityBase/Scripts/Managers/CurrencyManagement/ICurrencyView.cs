using UnityBase.UI.ButtonCore;
using UnityEngine;

namespace UnityBase.Manager
{
    public interface ICurrencyView
    {
        public Transform ViewTransform { get; }
        public Transform CoinIconT { get; }
        public void UpdateView(int val);
    }
    public interface ICurrencyView<out TAnim> : ICurrencyView where TAnim : IViewAnimation
    {
        public IViewBehaviour<TAnim> ViewBehaviour { get; }
    }
    
    public interface ICurrencyView<out TAnim, out TModel, out TData> : ICurrencyView<TAnim> where TAnim : IViewAnimation where TModel : IViewModel<TData> where TData : struct
    {
        public IViewBehaviour<TAnim, TModel, TData> ViewBehaviour { get; }
    }
}