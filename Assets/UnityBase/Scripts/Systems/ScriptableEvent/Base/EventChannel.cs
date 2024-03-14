using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.ScriptableEvent
{   
    [CreateAssetMenu(menuName = "Game/ScriptableEvents/EventChannel")]
    public class EventChannel : EventChannel<Empty> { }
    public readonly struct Empty { }

    public abstract class EventChannel<T> : ScriptableObject
    {
        private readonly HashSet<EventListener<T>> _listeners = new HashSet<EventListener<T>>();
        public void Invoke(T value)
        {
            foreach (var observer in _listeners)
            {
                observer.Invoke(value);
            }
        }

        public void AddListener(EventListener<T> listener) => _listeners.Add(listener);

        public void RemoveListener(EventListener<T> listener) => _listeners.Remove(listener);
    }   
}