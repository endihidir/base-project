using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    [DisallowMultipleComponent]
    public abstract class ButtonUI : MonoBehaviour, IButtonUI
    {
        [SerializeField, ReadOnly] private Button _button;
        
        private EventTrigger _eventTrigger;

        protected IButtonBehaviour _buttonBehaviour;
        public Button Button => _button;
        public IButtonBehaviour ButtonBehaviour => _buttonBehaviour;
        public virtual Transform Transform => transform;
        
        
#if UNITY_EDITOR
        private void OnValidate() => TryGetComponent(out _button);
#endif

        [Inject]
        public void Construct(IButtonBehaviourFactory buttonButtonBehaviourFactory)
        {
            Initialize(buttonButtonBehaviourFactory);
            _eventTrigger ??= gameObject.AddComponent<EventTrigger>();
            CreateEventTriggers();
        }
        protected abstract void Initialize(IButtonBehaviourFactory buttonBehaviourFactory);

        private void OnEnable() => _button.onClick.AddListener(OnClickButton);
        private void OnDisable() => _button.onClick.RemoveListener(OnClickButton);
        protected virtual void OnClickButton() => ButtonBehaviour?.OnClick();
        protected virtual void OnPointerDown()
        {
            if(!Button.IsInteractable()) return;
            
            ButtonBehaviour?.OnPointerDown();
        }
        protected virtual void OnPointerUp()
        {
            if(!Button.IsInteractable()) return;
            
            ButtonBehaviour?.OnPointerUp();
        }

        public void SetActive(bool value) => gameObject.SetActive(value);
        public void SetInteractable(bool value) => _button.interactable = value;
        public void SetRaycastTarget(bool value) => _button.image.raycastTarget = value;

        protected virtual void CreateEventTriggers()
        {
            AddEventTrigger(_eventTrigger, EventTriggerType.PointerDown, OnPointerDown);
            AddEventTrigger(_eventTrigger, EventTriggerType.PointerUp, OnPointerUp);
        }
        protected void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        public virtual void OnDestroy()
        {
            _eventTrigger?.triggers?.Clear();
            ButtonBehaviour?.Dispose();
        }
    }
}