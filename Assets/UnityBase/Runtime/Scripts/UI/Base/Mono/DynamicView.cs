using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public abstract class DynamicMonoView : MonoView, IDynamicView
    {
        [SerializeField] protected RectTransform _rectTransform;
        protected void Awake() => _rectTransform ??= GetComponent<RectTransform>();
        
        public abstract void OpenView();
        public abstract void CloseView();
        public abstract void OpenViewInstantly();
        public abstract void CloseViewInstantly();
    }
    public interface IDynamicView
    {
        public void OpenView();
        public void CloseView();
        public void OpenViewInstantly();
        public void CloseViewInstantly();
    }
}