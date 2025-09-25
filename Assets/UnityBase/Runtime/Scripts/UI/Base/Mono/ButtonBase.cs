using NaughtyAttributes;
using Unity.VisualScripting;
using UnityBase.Runtime.Factories;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    [DisallowMultipleComponent]
    public abstract class ButtonBase : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Button _button;
        [SerializeField, ReadOnly] private EventTrigger _eventTrigger;
        
        protected IButtonAnimation _buttonAnimation;
        protected IButtonAction _buttonAction;

        protected Button Button => _button ??= GetComponentInChildren<Button>();
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            TryGetComponent(out _button);
            TryGetComponent(out _eventTrigger);
            
            if (_button && _eventTrigger)
            {
                EditorUtility.SetDirty(this);
            }
        }
#endif

        [Inject]
        public void Construct(IOwnerContextFactory ownerContextFactory)
        {
            Initialize(ownerContextFactory);
            
            CreateEventTriggers();
        }
        protected abstract void Initialize(IOwnerContextFactory ownerContextFactory);

        private void OnEnable() => _button.onClick.AddListener(OnClickButton);
        private void OnDisable() => _button.onClick.RemoveListener(OnClickButton);

        private async void OnClickButton()
        {
            if (_buttonAnimation != null)
            {
                await _buttonAnimation.Click();
                
                _buttonAction?.OnClick();
            }
            else
            {
                _buttonAction?.OnClick();
            }
        }
        private void CreateEventTriggers()
        {
            var isThereEventTrigger = _eventTrigger ?? TryGetComponent(out _eventTrigger);

            if (!isThereEventTrigger)
            {
                _eventTrigger = transform.AddComponent<EventTrigger>();
            }
            
            AddEventTrigger(EventTriggerType.PointerDown, OnPointerDown);
            AddEventTrigger(EventTriggerType.PointerUp, OnPointerUp);
        }

        private async void OnPointerDown()
        {
            if (!Button.IsInteractable()) return;

            if (_buttonAnimation != null)
            {
                await _buttonAnimation.PointerDown();
                
                _buttonAction?.OnPointerDown();
            }
            else
            {
                _buttonAction?.OnPointerDown();
            }
        }

        private async void OnPointerUp()
        {
            if (!Button.IsInteractable()) return;

            if (_buttonAnimation != null)
            {
                await _buttonAnimation.PointerUp();
                
                _buttonAction?.OnPointerUp();
            }
            else
            {
                _buttonAction?.OnPointerUp();
            }
        }
        
        protected void AddEventTrigger(EventTriggerType eventType, UnityEngine.Events.UnityAction action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(_ => action());
            _eventTrigger.triggers.Add(entry);
        }

        protected virtual void OnDestroy()
        {
            _eventTrigger?.triggers?.Clear();
            _buttonAction?.Dispose();
            _buttonAnimation?.Dispose();
        }
    }
}