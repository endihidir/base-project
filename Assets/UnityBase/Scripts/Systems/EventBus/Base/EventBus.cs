using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityBase.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly IDictionary<int, HashSet<IEventBinding<T>>> _bindingDictionary = new Dictionary<int, HashSet<IEventBinding<T>>>();

        public static void AddListener(EventBinding<T> binding, int channel = 0)
        {
            if (!_bindingDictionary.TryGetValue(channel, out var bindings))
            {
                bindings = new HashSet<IEventBinding<T>>();

                _bindingDictionary[channel] = bindings;
            }

            bindings.Add(binding);

            _bindingDictionary[channel] = bindings.OrderBy(x => x.Priority).ToHashSet();
        }

        public static void RemoveListener(EventBinding<T> binding, int channel = 0, bool throwException = false)
        {
            if (_bindingDictionary.TryGetValue(channel, out var bindings))
            {
                binding.ClearBindingData();
                bindings.Remove(binding);
                return;
            }

            if (throwException)
            {
                throw new ArgumentException($"Channel: {channel} is not available in the bindings list.");
            }
        }

        public static void Invoke(T @event, int channel = 0, bool throwException = false)
        {
            if (_bindingDictionary.TryGetValue(channel, out var bindings))
            {
                foreach (var binding in bindings)
                {
                    binding.OnEvent.Invoke(@event);
                    binding.OnEventNoArgs.Invoke();
                }

                return;
            }

            if (throwException)
            {
                throw new ArgumentException($"Channel: {channel} is not available in the bindings list.");
            }
        }

        private static void Clear()
        {
            //Debug.Log($"Clearing {typeof(T).Name} bindings");

            foreach (var bindings in _bindingDictionary)
            {
                foreach (var binding in bindings.Value)
                {
                    binding.ClearBindingData();
                }

                bindings.Value.Clear();
            }

            _bindingDictionary.Clear();
        }
    }
}