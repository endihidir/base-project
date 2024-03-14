using UnityEngine;

namespace UnityBase.UI.Base
{
    public abstract class BaseUIElements : MonoBehaviour
    {

#if UNITY_EDITOR
        protected virtual void OnValidate() { }

#endif

        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void Start() { }
        protected virtual void OnDestroy() { }
    }
}