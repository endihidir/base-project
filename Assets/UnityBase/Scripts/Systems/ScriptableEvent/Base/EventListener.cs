using UnityEngine;
using UnityEngine.Events;

namespace UnityBase.ScriptableEvent
{
    public abstract class EventListener<T> : MonoBehaviour
    {
        [SerializeField] private EventChannel<T> _eventChannel;
        [SerializeField] private UnityEvent<T> _unityEvent;

        protected void OnEnable()
        {
            _eventChannel.AddListener(this);
        }

        protected void OnDisable()
        {
            _eventChannel.RemoveListener(this);
        }

        public void Invoke(T value)
        {
            _unityEvent.Invoke(value);
        }
    }

    public class EventListener : EventListener<Empty>
    {
    }
}