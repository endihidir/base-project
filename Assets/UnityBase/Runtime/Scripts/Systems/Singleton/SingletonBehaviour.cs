using UnityEngine;

namespace UnityBase.Singleton
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                _instance = FindObjectOfType<T>(true);

                if (_instance != null) return _instance;
                
                var go = new GameObject(typeof(T).Name + " Auto-Generated");
                _instance = go.AddComponent<T>();
                
                return _instance;
            }
        }

        protected bool HasMultipleInstances()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return true;
            }

            _instance = FindObjectOfType<T>();
            
            return false;
        }
    }
}