using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.ServiceLocator
{
    public sealed class ServiceHandler
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        public IEnumerable<object> RegisteredServices => _services.Values;
        
        public T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var obj))
            {
                return obj as T;
            }
            
            throw new ArgumentException($"ServiceManager.Get: Service of type {type.FullName} not registered");
        }
        
        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                
                return true;
            }

            service = null;

            return false;
        }

        public ServiceHandler Register<T>(T service)
        {
            var type = typeof(T);

            if (!_services.TryAdd(type, service))
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");

            return this;
        }

        public ServiceHandler Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
                throw new ArgumentException("Type of service does not match type of service interface", nameof(service));

            if (!_services.TryAdd(type, service))
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");

            return this;
        }
    }
}