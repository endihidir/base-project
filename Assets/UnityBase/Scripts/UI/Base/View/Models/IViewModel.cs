using UnityBase.Observable;

namespace UnityBase.UI.ViewCore
{
    public interface IViewModel
    {
        public void Dispose();
    }
    
    public interface IViewModel<in TData> : IViewModel where TData : struct
    {
        public void ChangeValue(TData value);
        public void OnValueChanged(TData value);
        public T Serialize<T>() where T : struct;
        public void Deserialize<T>(T value) where T : struct;
    }
}