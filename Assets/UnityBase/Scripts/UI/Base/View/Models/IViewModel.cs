using UnityBase.Observable;

namespace UnityBase.UI.ViewCore
{
    public interface IViewModel
    {
        public void Dispose();
    }
    
    public interface IViewModel<TData> : IViewModel where TData : struct
    {
        public Observable<TData> Values { get; }
        public void OnValueChanged(TData value);
        public T Serialize<T>() where T : struct;
        public void Deserialize<T>(T value) where T : struct;
    }
}