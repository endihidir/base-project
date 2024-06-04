using System;
using UnityBase.Observable;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ViewModel<TData> : IViewModel<TData> where TData : struct
    {
        public Observable<TData> Views { get; }
        protected ViewModel(TData value, Action<TData> onValueChanged = null)
        {
            Views = new Observable<TData>(value, onValueChanged);
        }
        
        public abstract TData Serialize();
        public abstract void Deserialize(TData value);
        public virtual void Dispose() => Views?.Dispose();
    }

    public class CoinViewModel : ViewModel<int>
    {
        public CoinViewModel(int value, Action<int> onValueChanged = null) : base(value, onValueChanged)
        {
        }

        public void Configure()
        {
            
        }

        public override int Serialize()
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(int value)
        {
            throw new NotImplementedException();
        }
    }

    public struct CoinData
    {
        
    }
}