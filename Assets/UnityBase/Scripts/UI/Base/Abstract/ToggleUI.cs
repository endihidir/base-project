using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.UI.ToggleCore
{
    public abstract class ToggleUI : MonoBehaviour
    {
        [SerializeField] protected RectTransform _handleRectTransform;

        [SerializeField] protected Toggle _toggle;

        [SerializeField] protected Image _backgroundImage;

        [SerializeField] protected float _toggleMovementDuration = 0.4f;

        [SerializeField] protected Ease _toggleMovementEase = Ease.InOutBack;

        protected float _handleXPos;

        private Tween _handleMoveTween, _backgroundFillTween;

#if UNITY_EDITOR
        protected void OnValidate() => _toggle = GetComponent<Toggle>();
#endif

        protected void Awake() => _handleXPos = _handleRectTransform.anchoredPosition.x;

        protected void OnEnable() => _toggle.onValueChanged.AddListener(OnValueChanged);

        protected void OnDisable() => _toggle.onValueChanged.RemoveListener(OnValueChanged);

        protected virtual void OnValueChanged(bool value)
        {
            var handlePos = value ? _handleXPos * -1f : _handleXPos;

            _handleMoveTween.Kill();
            _handleMoveTween = _handleRectTransform.DOAnchorPosX(handlePos, _toggleMovementDuration)
                .SetEase(_toggleMovementEase).SetUpdate(true);

            _backgroundFillTween.Kill();
            _backgroundFillTween = _backgroundImage.DOFillAmount(value ? 1f : 0f, _toggleMovementDuration)
                .SetEase(_toggleMovementEase).SetUpdate(true);
        }

        public virtual void UpdateToggle() { }
        public virtual void SetInteractable(bool enable) => _toggle.interactable = enable;
        public virtual void SetActive(bool enable) => gameObject.SetActive(enable);

        protected void OnDestroy()
        {
            _handleMoveTween.Kill();
            _backgroundFillTween.Kill();
        }
    }
}