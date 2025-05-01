using System;
using System.Collections.Generic;
using UnityBase.Extensions;

namespace UnityBase.BlackboardCore
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        private readonly string _name;
        
        private readonly int _hashedKey;

        public BlackboardKey(string name)
        {
            _name = name;
            _hashedKey = name.ComputeFnv1AHash();
        }
        public bool Equals(BlackboardKey other) => _hashedKey.Equals(other._hashedKey);
        public override bool Equals(object obj) => obj is BlackboardKey other && Equals(other);
        public override int GetHashCode() => _hashedKey;
        public override string ToString() => _name;
        public static bool operator ==(BlackboardKey lhs, BlackboardKey rhs) => lhs._hashedKey.Equals(rhs._hashedKey);
        public static bool operator !=(BlackboardKey lhs, BlackboardKey rhs) => !(lhs == rhs);
    }

    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj) => obj is BlackboardEntry<T> other && other.Key == Key;
        public override int GetHashCode() => Key.GetHashCode();
    }
    
    public class GlobalBlackboard : IBlackboard
    {
        private Dictionary<string, BlackboardKey> _keyRegistry = new();
        
        private Dictionary<BlackboardKey, object> _entries = new();

        public event Action<BlackboardKey> OnKeyAdded;
        public event Action<BlackboardKey> OnKeyUpdated;

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (_entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> castedEntry)
            {
                value = castedEntry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public void SetValue<T>(BlackboardKey key, T value)
        {
            _entries[key] = new BlackboardEntry<T>(key, value);
            
            OnKeyUpdated?.Invoke(key);
        }

        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return default;

            if (!_keyRegistry.TryGetValue(keyName, out var blackboardKey))
            {
                blackboardKey = new BlackboardKey(keyName);
                _keyRegistry[keyName] = blackboardKey;
                OnKeyAdded?.Invoke(blackboardKey);
            }
            
            return blackboardKey;
        }

        public bool ContainsKey(BlackboardKey key) => _entries.ContainsKey(key);
        public void Remove(BlackboardKey key) => _entries.Remove(key);

        public void Debug()
        {
            foreach (var entry in _entries)
            {
                var entryType = entry.Value.GetType();

                if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == typeof(BlackboardEntry<>))
                {
                    var valueProperty = entryType.GetProperty("Value");
                    
                    if(valueProperty == null) continue;

                    var value = valueProperty.GetValue(entry.Value);
                    
                    UnityEngine.Debug.LogError($"Key: {entry.Key}, Value: {value}");
                }
            }
        }
    }

    public interface IBlackboard
    {
        public event Action<BlackboardKey> OnKeyAdded;
        public event Action<BlackboardKey> OnKeyUpdated; 
        public bool TryGetValue<T>(BlackboardKey key, out T value);
        public void SetValue<T>(BlackboardKey key, T value);
        public BlackboardKey GetOrRegisterKey(string keyName);
        public bool ContainsKey(BlackboardKey key);
        public void Remove(BlackboardKey key);
        public void Debug();
    }
}