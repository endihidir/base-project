using DG.Tweening;
using UnityBase.UI.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonBehaviour : BaseUIElements, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] protected Button _button;

        [SerializeField] protected Image _image;

        [SerializeField] private bool _isIndependentFromGroup;
        public bool IsIndependentFromGroup => _isIndependentFromGroup;
        public RectTransform RectTransform => _image.rectTransform;

        protected Tween _clickTween;

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
        }

#endif

        protected override void OnEnable() => _button.onClick.AddListener(OnClick);

        protected override void OnDisable() => _button.onClick.RemoveListener(OnClick);

        protected abstract void OnClick();

        public virtual void UpdateButton() { }

        public virtual void SetRaycastTarget(bool enable) => _image.raycastTarget = enable;

        public virtual void SetInteractable(bool enable) => _button.interactable = enable;

        public virtual void SetActive(bool enable) => gameObject.SetActive(enable);
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;

            _clickTween.Kill();
            _clickTween = transform.DOScale(1.1f, 0.1f).SetUpdate(true);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;

            _clickTween.Kill();
            _clickTween = transform.DOScale(1f, 0.1f).SetUpdate(true);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _clickTween?.Kill();
        }
    }
}