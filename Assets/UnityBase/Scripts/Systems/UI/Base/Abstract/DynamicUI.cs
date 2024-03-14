using DG.Tweening;
using Sirenix.OdinInspector;
using UnityBase.UI.Base;
using UnityBase.UI.ButtonCore;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public abstract class DynamicUI : BaseUIElements
    {
        [Title("General Settings")] 
        
        [SerializeField] protected float _openDuration = 0.5f;

        [SerializeField] protected float _closeDuration = 0.5f;

        [SerializeField] protected float _openDelay, _closeDelay;

        [SerializeField] protected Ease _openEase = Ease.InOutQuad;

        [SerializeField] protected Ease _closeEase = Ease.InOutQuad;

        [SerializeField] private bool _unscaledTime;

        [SerializeField, Required] protected RectTransform _uiHandler;

        [ReadOnly] [SerializeField] protected RectTransform _rectTransform;

        [ReadOnly] [SerializeField] protected ButtonBehaviour[] _buttonBehaviours;

        public bool UnscaledTime
        {
            get => _unscaledTime;
            set => _unscaledTime = value;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _rectTransform = GetComponent<RectTransform>();

            _buttonBehaviours = GetComponentsInChildren<ButtonBehaviour>(true);
        }
#endif
        protected override void Awake()
        {
            base.Awake();

            _rectTransform ??= GetComponent<RectTransform>();
        }

        public virtual void OpenUI() => EnableButtonBehaviours();
        public virtual void CloseUI() => DisableButtonBehaviours();
        public virtual void OpenUIDirectly() => EnableButtonBehaviours();
        public virtual void CloseUIDirectly() => DisableButtonBehaviours();

        private void EnableButtonBehaviours()
        {
            if(_buttonBehaviours is null) return;

            foreach (var buttonBehaviour in _buttonBehaviours)
            {
                if (buttonBehaviour.IsIndependentFromGroup) continue;
            
                buttonBehaviour.SetActive(true);
            }
        }
        
        private void DisableButtonBehaviours()
        {
            if(_buttonBehaviours is null) return;

            foreach (var buttonBehaviour in _buttonBehaviours)
            {
                if (buttonBehaviour.IsIndependentFromGroup) continue;
            
                buttonBehaviour.SetActive(false);
            }
        }
    }
}