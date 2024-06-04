using UnityBase.Observable;

namespace UnityBase.UI.ButtonCore
{
    public interface IViewModel<TData> where TData : struct
    {
        public Observable<TData> Views { get; }
        public TData Serialize();
        public void Deserialize(TData value);
        public void Dispose();
    }
}