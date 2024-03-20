using System;
using UnityBase.Pool;
using UnityEngine;

namespace UnityBase.PopUpCore
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopUp : MonoBehaviour, IPoolable
    {
        [Header("ANIMATION DEPENDENCIES")] [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField] protected RectTransform _rectTransform;

        [SerializeField] protected Transform _popUpHandler;

        [SerializeField] protected bool _isSettingsPopUp;
        public bool IsSettingsPopUp => _isSettingsPopUp;
        public Component PoolableObject => this;
        public virtual bool IsActive => isActiveAndEnabled;
        public virtual bool IsUnique => true;

        private Vector2 _defaultSizeDelta;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }
#endif
        private void Awake() => _defaultSizeDelta = _rectTransform.sizeDelta;
        
        public void ResetPopUpSize()
        {
            _rectTransform.sizeDelta = _defaultSizeDelta;
            transform.localScale = Vector3.one;
        }

        public abstract void Show(float duration, float delay, Action onComplete);
        public abstract void Hide(float duration, float delay, Action onComplete);

        protected void SetGroupSettings(float alpha, bool interactable, bool raycastTarget)
        {
            _canvasGroup.alpha = alpha;
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = raycastTarget;
        }

        protected virtual void OnDestroy() { }
    }
}