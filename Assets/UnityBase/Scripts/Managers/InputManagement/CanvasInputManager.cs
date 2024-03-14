using System;
using Sirenix.OdinInspector;
using UnityBase.EventBus;
using UnityBase.Manager.Data;
using UnityBase.Singleton;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityBase.Manager
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(Image))]
    public class CanvasInputManager : PersistentSingletonBehaviour<CanvasInputManager>, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public event Action<PointerEventData> OnMouseDown, OnMouseDrag, OnMouseUp;
        public event Action<Vector2> GetMouseDelta;

        [SerializeField] private float _mouseSensitivity = 40;
        [ReadOnly] [ShowInInspector] public bool IsInputEnabled { get; private set; }
        
        private EventBinding<GameStateData> _gameStateBinding = new EventBinding<GameStateData>();

    #if UNITY_EDITOR
        private void OnValidate()
        {
            PrepareDependencies();
        }
    #endif
        protected void Awake()
        {
            if(HasMultipleInstances()) return;
            
            PrepareDependencies();
        }

        public bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();

        private void PrepareDependencies()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -3;

            var canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1f;

            var image = GetComponent<Image>();
            var color = Color.white;
            color.a = 0f;
            image.color = color;
            
        }

        private void OnEnable()
        {
            _gameStateBinding.Add(OnStartGameStateTransition);
            
            EventBus<GameStateData>.AddListener(_gameStateBinding, GameStateData.GetChannel(TransitionState.Start));
        }

        private void OnDisable()
        {
            _gameStateBinding.Remove(OnStartGameStateTransition);
            
            EventBus<GameStateData>.RemoveListener(_gameStateBinding, GameStateData.GetChannel(TransitionState.Start));
        }

        private void OnStartGameStateTransition(GameStateData gameStateData)
        {
            IsInputEnabled = gameStateData.EndState is GameState.GamePlayState or GameState.GameTutorialState;

            if (!IsInputEnabled)
            {
                ResetInputData();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(!IsInputEnabled) return;

            OnMouseDown?.Invoke(eventData);

            var mouseDelta = eventData.delta * (1f / _mouseSensitivity);
            
            GetMouseDelta?.Invoke(mouseDelta);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if(!IsInputEnabled) return;

            OnMouseDrag?.Invoke(eventData);
            
            var mouseDelta = eventData.delta * (1f / _mouseSensitivity);
            
            GetMouseDelta?.Invoke(mouseDelta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(!IsInputEnabled) return;
            
            OnMouseUp?.Invoke(eventData);
            
            var mouseDelta = eventData.delta * (1f / _mouseSensitivity);
            
            GetMouseDelta?.Invoke(mouseDelta);
        }

        private void ResetInputData()
        {
            
        }
    }   
}