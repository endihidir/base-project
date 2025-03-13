using NaughtyAttributes;
using UnityBase.UI.ViewCore;
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

        [Inject]
        private void Construct(IViewBehaviourFactory viewBehaviourFactory, IObjectResolver resolver)
        {
            viewBehaviourFactory.UpdateResolver(resolver);
            Initialize(viewBehaviourFactory);
        }
        protected abstract void Initialize(IViewBehaviourFactory viewBehaviourFactory);

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

