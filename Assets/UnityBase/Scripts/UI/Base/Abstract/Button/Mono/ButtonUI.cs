using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonUI : MonoBehaviour, IButtonUI, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField, ReadOnly] private Button _button;
        
        protected IButtonBehaviour _buttonBehaviour;

        public IButtonBehaviour ButtonBehaviour => _buttonBehaviour;
        
        public Button Button => _button;
        
        public virtual Transform Transform => transform;
        
        [Inject]
        public void Construct(IButtonBehaviourFactory buttonBehaviourFactory) => Initialize(buttonBehaviourFactory);

        public void SetActive(bool value) => gameObject.SetActive(value);

        public void SetInteractable(bool value) => _button.interactable = value;

        public void SetRaycastTarget(bool value) => _button.image.raycastTarget = value;
        public void EditorInitialize() => TryGetComponent(out _button);
        

        protected abstract void Initialize(IButtonBehaviourFactory buttonBehaviourFactory);
        public abstract void OnPointerClick(PointerEventData eventData);
        public abstract void OnPointerDown(PointerEventData eventData);
        public abstract void OnPointerUp(PointerEventData eventData);
        public virtual void OnDestroy() => _buttonBehaviour?.Dispose();
    }
}