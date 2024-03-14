using System;

namespace UnityBase.Observable
{
    [Serializable]
    public class Observable<T>
    {
        private T _value;
        
        public event Action<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set => Set(value);
        }

        public static implicit operator T(Observable<T> observable) => observable._value;

        public Observable(T value, Action<T> onOnValueChanged = null)
        {
            _value = value;

            if (onOnValueChanged != null)
            {
                OnValueChanged += onOnValueChanged;
            }
        }

        public void Set(T value)
        {
            if (Equals(_value, value)) return;

            _value = value;
            
            Invoke();
        }

        public void Invoke()
        {
            OnValueChanged?.Invoke(_value);
        }

        public void AddListener(Action<T> handler)
        {
            OnValueChanged += handler;
        }

        public void RemoveListener(Action<T> handler)
        {
            OnValueChanged -= handler;
        }

        public void Dispose()
        {
            OnValueChanged = null;
            _value = default;
        }
    }
}