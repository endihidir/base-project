using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityBase.ServiceLocator
{
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _global;
        private static Dictionary<Scene, ServiceLocator> _sceneContainers;
        private static List<GameObject> _tmpSceneGameObjects;
        
        private readonly ServiceHandler _services = new ServiceHandler();

        private const string GLOBAL_SERVICE_LOCATOR_NAME = "ServiceLocator [Global]";
        private const string SCENE_SERVICE_LOCATOR_NAME = "ServiceLocator [Scene]";

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_global == this)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: Already configured as global", this);
            }
            else if(_global != null)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global", this);
            }
            else
            {
                _global = this;
                if (!dontDestroyOnLoad) return;
                gameObject.transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (_sceneContainers.ContainsKey(scene))
            {
                Debug.LogWarning("ServiceLocator.ConfigureForScene: Another ServiceLocator is already configured for this scene", this);
                return;
            }
            
            _sceneContainers.Add(scene, this);
        }

        public static ServiceLocator Global
        {
            get
            {
                if (_global is not null) return _global;

                if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is { } found)
                {
                    found.BootstrapOnDemand();
                    return _global;
                }

                var container = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocator));
                
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();

                return _global;
            }
        }

        public static ServiceLocator For(MonoBehaviour mb)
        {
            return mb.GetComponentInParent<ServiceLocator>() ?? ForSceneOf(mb) ?? Global;
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            Scene scene = mb.gameObject.scene;

            if (_sceneContainers.TryGetValue(scene, out ServiceLocator container) && container != mb)
            {
                return container;
            }
            
            _tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(_tmpSceneGameObjects);

            foreach (var go in _tmpSceneGameObjects.Where(go => go.GetComponent<ServiceLocatorSceneBootstrapper>() is not null))
            {
                if (go.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper) && bootstrapper.Container != mb)
                {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Container;
                }
            }

            return Global;
        }

        public ServiceLocator Register<T>(T service)
        {
            _services.Register(service);
            return this;
        }

        public ServiceLocator Register(Type type, object service)
        {
            _services.Register(type, service);
            return this;
        }

        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return this;

            if (TryGetNextInHierarch(out ServiceLocator container))
            {
                container.Get(out service);
                return this;
            }

            throw new ArgumentException($"ServiceLocator.Get: Service of type {typeof(T).FullName} not registered");
        }
        
        public T Get<T>() where T : class
        {
            var type = typeof(T);

            if (TryGetService(type, out T service)) return service;

            if (TryGetNextInHierarch(out ServiceLocator container))
                return container.Get<T>();

            throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            return _services.TryGet(out service);
        }

        private bool TryGetService<T>(Type type, out T service) where T : class
        {
            return _services.TryGet(out service);
        }

        private bool TryGetNextInHierarch(out ServiceLocator container)
        {
            if (this == _global)
            {
                container = null;
                return false;
            }

            container = transform.parent.GetComponentInParent<ServiceLocator>() ?? ForSceneOf(this);
            
            return container != null;
        }

        private void OnDestroy()
        {
            if (this == _global)
            {
                _global = null;
            }
            else if(_sceneContainers.ContainsValue(this))
            {
                _sceneContainers.Remove(gameObject.scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _global = null;
            _sceneContainers = new Dictionary<Scene, ServiceLocator>();
            _tmpSceneGameObjects = new List<GameObject>();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/ServiceLocator/Add Global")]
        static void AddGlobal()
        {
            var go = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorGlobalBootstrapper));
        }
        
        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        static void AddScene()
        {
            var go = new GameObject(SCENE_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorSceneBootstrapper));
        }
#endif
        
    }
}