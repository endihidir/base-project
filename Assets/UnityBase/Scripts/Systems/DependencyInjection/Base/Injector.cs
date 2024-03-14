using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityBase.Singleton;
using UnityEngine;

namespace DependencyInjection
{
    [DefaultExecutionOrder(-1000)]
    public class Injector : SingletonBehaviour<Injector>
    {
        [SerializeField] private bool _showLogs = false;
        
        private const BindingFlags k_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();
        
        private void Awake()
        {
            if (HasMultipleInstances()) return;

            var monoBehaviours = FindAllMonoBehaviours();

            var providers = monoBehaviours.OfType<IDependencyContainer>();

            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }

            var injectables = monoBehaviours.Where(IsInjectable);

            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }
        
        private static IEnumerable<MonoBehaviour> FindAllMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        }
        
        private void RegisterProvider(IDependencyContainer container)
        {
            var methods = container.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(container, null);
                
                if (providedInstance is not null)
                {
                    if (!_registry.ContainsKey(returnType))
                    {
                        _registry.Add(returnType, providedInstance);
                    
                        if(_showLogs)
                            Debug.Log($"Registered {returnType.Name} from {container.GetType().Name}");
                    }
                    else
                    {
                        if(_showLogs)
                            Debug.Log($"Already {returnType.Name} from {container.GetType().Name} registered.");
                    }
                }
                else
                {
                    throw new Exception($"Container {container.GetType().Name} returned null for {returnType.Name}");
                }
            }
        }

        private void Inject(MonoBehaviour instance)
        {
            var type = instance.GetType();
            
            InjectFields(instance, type);

            InjectMethods(instance, type);

            InjectProperties(instance, type);
        }

        private void InjectFields(MonoBehaviour instance, Type type)
        {
            var injectableFields = type.GetFields(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableField in injectableFields)
            {
                if (injectableField.GetValue(instance) != null)
                {
                    Debug.LogWarning($"[Injector] field '{ injectableField.Name }' of class '{ type.Name }'");
                    continue;
                }
                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);

                if (resolvedInstance is null)
                {
                    throw new Exception($"Failed to resolve {fieldType.Name} for {type.Name}");
                }

                injectableField.SetValue(instance, resolvedInstance);

                if (_showLogs)
                    Debug.Log($"Injected {fieldType.Name} from {type.Name}");
            }
        }


        private void InjectMethods(MonoBehaviour instance, Type type)
        {
            var injectableMethods = type.GetMethods(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableMethod in injectableMethods)
            {
                var methodParameterInfos = injectableMethod.GetParameters();

                var resolvedMethodParameters = new object[methodParameterInfos.Length];

                for (int i = 0; i < methodParameterInfos.Length; i++)
                {
                    var parameterInfo = methodParameterInfos[i];

                    var parameterType = parameterInfo.ParameterType;

                    var resolvedInstance = Resolve(parameterType);

                    resolvedMethodParameters[i] = resolvedInstance ?? throw new Exception($"Failed to resolve {parameterType.Name} for {type.Name}");

                    if (_showLogs)
                        Debug.Log($"Injected {parameterType.Name} from {type.Name}");
                }

                injectableMethod.Invoke(instance, resolvedMethodParameters);
            }
        }
        
        private void InjectProperties(MonoBehaviour instance, Type type)
        {
            var injectableProperties = type.GetProperties(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableProperty in injectableProperties)
            {
                var propertyType = injectableProperty.PropertyType;
                
                var resolvedInstance = Resolve(propertyType);
                
                if (resolvedInstance is null)
                {
                    throw new Exception($"Failed to resolve {propertyType.Name} for {type.Name}");
                }
                
                injectableProperty.SetValue(instance, resolvedInstance);
                
                if (_showLogs)
                    Debug.Log($"Injected {propertyType.Name} from {type.Name}");
            }
        }
        
        public void ValidateDependencies() 
        {
            var monoBehaviours = FindAllMonoBehaviours();
            var providers = monoBehaviours.OfType<IDependencyContainer>();
            var providedDependencies = GetProvidedDependencies(providers);

            var invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(k_bindingFlags), (mb, field) => new {mb, field})
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");
            
            var invalidDependencyList = invalidDependencies.ToList();
            
            if (!invalidDependencyList.Any()) 
            {
                Debug.Log("[Validation] All dependencies are valid.");
            } 
            else 
            {
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");
                
                foreach (var invalidDependency in invalidDependencyList) 
                {
                    Debug.LogError(invalidDependency);
                }
            }
        }
        
        private HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyContainer> providers) 
        {
            var providedDependencies = new HashSet<Type>();
            foreach (var provider in providers) 
            {
                var methods = provider.GetType().GetMethods(k_bindingFlags);
                
                foreach (var method in methods) 
                {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                    
                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);
                }
            }

            return providedDependencies;
        }

        public void ClearDependencies() 
        {
            foreach (var monoBehaviour in FindAllMonoBehaviours()) 
            {
                var type = monoBehaviour.GetType();
                var injectableFields = type.GetFields(k_bindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (var injectableField in injectableFields) 
                {
                    injectableField.SetValue(monoBehaviour, null);
                }
            }
            
            Debug.Log("[Injector] All injectable fields cleared.");
        }
        
        private object Resolve(Type type)
        {
            _registry.TryGetValue(type, out var resolvedInstance);
            
            return resolvedInstance;
        }

        private static bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
    }
}