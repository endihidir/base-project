using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public abstract class DynamicUI : MonoBehaviour, IDynamicView
    {
        [ReadOnly] [SerializeField] protected RectTransform _rectTransform;
        
#if UNITY_EDITOR
        protected void OnValidate() => _rectTransform = GetComponent<RectTransform>();
#endif
        protected void Awake() => _rectTransform ??= GetComponent<RectTransform>();

        public abstract void OpenView();
        public abstract void CloseView();
        public abstract void OpenViewInstantly();
        public abstract void CloseViewInstantly();
        protected abstract void OnDestroy();
    }
    public interface IDynamicView
    {
        public void OpenView();
        public void CloseView();
        public void OpenViewInstantly();
        public void CloseViewInstantly();
    }
}

