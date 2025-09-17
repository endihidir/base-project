using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;

namespace __Funflare.Scripts.RemoteConfigsSO
{
    public abstract class BaseScriptableRemoteConfig<T> : ScriptableObject
    {
        [Header("Tick this if this Scriptable Objects is used for override test config")]
        [SerializeField] private bool isOverrideTestConfig;

        [Header("------------------------")] 
        
        [HideIf("isOverrideTestConfig")] 
        [SerializeField] public string remoteKey;

        [HideIf("isOverrideTestConfig")] 
        [SerializeField] private bool useDefaultIfKeyNotPresent;

        [HideIf("isOverrideTestConfig")] 
        [SerializeField] private bool useCachedValueIfPossible;
        
#if UNITY_EDITOR
        
        [BoxGroup("Local Test")]
        [Header("Tick This To Test Default Values")] 
        [Tooltip("Ignores Remote Values")]
        [ValidateInput("Validator_IsActive", "LOCAL TEST IS ACTIVE")]
        [SerializeField] private bool testLocal;
        
        private bool Validator_IsActive(bool active)
        {
            return !active;
        }
#endif


        protected bool _isJsonCopyDeactivated;
        private string CachedValueSaveKey => string.Concat("es3_rc_cached_", remoteKey);

        public bool UseDefaultIfKeyNotPresent => useDefaultIfKeyNotPresent;

        [field:SerializeField] private T DefaultValue { get; set; }

        public bool HasRemoteValue()
        {
            return false /*FunflareAnalytics.RemoteConfig.ConfigExist(remoteKey)*/;
        }

        public bool HasCachedValue()
        {
            return false /*PlayerPrefs.KeyExists(CachedValueSaveKey)*/;
        }

        public void CacheDefaultValue()
        {
            /*ES3.Save(CachedValueSaveKey, DefaultValue);
            ES3KeysUtil.InvalidateEs3Key();*/
        }

        public void CacheCurrentValue()
        {
            /*ES3.Save(CachedValueSaveKey, Value);
            ES3KeysUtil.InvalidateEs3Key();*/
        }

        private T _cachedValue;
        private bool _hasLoadedCachedValue;

        public T CachedValue
        {
            get
            {
                if (HasCachedValue())
                {
                    if (!_hasLoadedCachedValue)
                    {
                        _hasLoadedCachedValue = true;
                        _cachedValue = default; //ES3.Load<T>(CachedValueSaveKey);
                    }

                    return _cachedValue;
                }

                Debug.LogError($"Trying To Get Cached RC Value That Does Not Exist {remoteKey} - {CachedValueSaveKey}");

                return default;
            }
        }

        public T Value
        {
            get
            {
#if UNITY_EDITOR
                if (testLocal)
                {
                    return DefaultValue;
                }
#endif

                if (useCachedValueIfPossible && HasCachedValue())
                {
                    return CachedValue;
                }

                /*if (FunflareAnalytics.RemoteConfig.GetConfig<T>(remoteKey, out var result))
                {
                    return result;
                }*/

                return DefaultValue;
            }
        }

#if UNITY_EDITOR
        [Button, DisableIf(nameof(_isJsonCopyDeactivated))]
        protected void CopyJsonToClipBoard()
        {
            var json = JsonConvert.SerializeObject(DefaultValue, Formatting.Indented);
            var str = "";
            str += $"Key: {remoteKey}";
            str += $"\n\n{json}";
            Debug.LogWarning($"Copied {remoteKey} json data.");
            GUIUtility.systemCopyBuffer = str;
        }
        
            
        
        [Button("RuntimeOnly Copy 'Remote Value' To Json")]
        protected void CopyRemoteValueToJson()   
        {
            var json = JsonUtility.ToJson(Value,true);
            var str = "";
            str += $"Key: {remoteKey}";
            str += $"\n\n{json}";
            Debug.LogWarning($"Copied Current {remoteKey} json data.");
            GUIUtility.systemCopyBuffer = str;
        }
#endif
    }
}