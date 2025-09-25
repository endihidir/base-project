using UnityBase.Runtime.Factories;
using UnityEngine;
using VContainer;

namespace UnityBase.UI.Dynamic
{
    public abstract class DynamicUI : MonoBehaviour, IDynamicUI
    {
        [SerializeField] protected RectTransform _rectTransform;
        protected void Awake() => _rectTransform ??= GetComponent<RectTransform>();

        private IOwnerContextFactory _ownerContextFactory;
        public IOwnerContext OwnerContext { get; private set; }

        [Inject]
        private void Construct(IOwnerContextFactory ownerContextFactory)
        {
            _ownerContextFactory = ownerContextFactory;
            
            OwnerContext = _ownerContextFactory.GetContext(this);
            
            Initialize();
        }
        
        protected abstract void Initialize();
        public abstract void OpenView();
        public abstract void CloseView();
        public abstract void OpenViewInstantly();
        public abstract void CloseViewInstantly();
        protected virtual void OnDestroy() => _ownerContextFactory?.Release(this);
    }
    public interface IDynamicUI
    {
        public IOwnerContext OwnerContext { get; }
        public void OpenView();
        public void CloseView();
        public void OpenViewInstantly();
        public void CloseViewInstantly();
    }
}