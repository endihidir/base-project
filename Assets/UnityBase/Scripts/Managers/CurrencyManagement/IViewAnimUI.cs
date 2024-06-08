using System;
using UnityBase.UI.ButtonCore;

namespace UnityBase.Manager
{
    public interface IViewUI
    {
        public Type Key { get; }
    }
    
    public interface IViewAnimUI<out TAnim> : IViewUI where TAnim : IViewAnimation
    {
        public IViewAnimBehaviour<TAnim> AnimBehaviour { get; }
    }
    
    public interface IViewModelUI<out TModel, TData> : IViewUI where TModel : IViewModel<TData> where TData : struct
    {
        public TData Value { get; set; }
        public IViewModelBehaviour<TModel, TData> ModelBehaviour { get; }
    }
    
}