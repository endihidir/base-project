using NaughtyAttributes;
using UnityBase.Runtime.Behaviours;
using UnityEngine;
using VContainer;

namespace UnityBase.UI.Dynamic
{
    public abstract class DynamicUI : MonoBehaviour, IDynamicUI
    {
        [ReadOnly] [SerializeField] protected RectTransform _rectTransform;
        
#if UNITY_EDITOR
        protected void OnValidate() => _rectTransform = GetComponent<RectTransform>();
#endif
        protected void Awake() => _rectTransform ??= GetComponent<RectTransform>();
        
        protected IOwnerBehaviourFactory _ownerFactory;

        [Inject]
        private void Construct(IOwnerBehaviourFactory ownerBehaviourFactory)
        {
            _ownerFactory = ownerBehaviourFactory;
            
            Initialize();
        }
        
        protected abstract void Initialize();

        public abstract void OpenView();
        public abstract void CloseView();
        public abstract void OpenViewInstantly();
        public abstract void CloseViewInstantly();
        protected abstract void OnDestroy();
    }
    public interface IDynamicUI
    {
        public void OpenView();
        public void CloseView();
        public void OpenViewInstantly();
        public void CloseViewInstantly();
    }
}