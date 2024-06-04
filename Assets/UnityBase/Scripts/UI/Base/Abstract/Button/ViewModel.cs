using System;
using UnityBase.Observable;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ViewModel<TData> : IViewModel<TData> where TData : struct
    {
        public Observable<TData> Views { get; }

        protected ViewModel(TData logic, Action<TData> onValueChanged = null)
        {
            Views = new Observable<TData>(logic, onValueChanged);
        }
        
        public abstract TData Serialize();
        
        public abstract void Deserialize(TData viewData);

        public virtual void Dispose() => Views?.Dispose();
    }
    
}