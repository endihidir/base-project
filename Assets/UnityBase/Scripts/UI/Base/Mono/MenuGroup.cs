using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityBase.EventBus;
using UnityBase.Manager.Data;
using UnityBase.UI.Config.SO;
using UnityBase.UI.Dynamic;
using UnityEngine;

namespace UnityBase.UI.Menu
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuGroup : MonoBehaviour
    {
        [SerializeField] private MenuGroupViewConfigSO _configSo;
        
        private Tween _canvasFadeTween;
        
        private CanvasGroup _canvasGroup;
        private IDynamicUI[] _dynamicViews;
        
        private EventBinding<GameStateData> _gameStateStartBinding = new();
        private EventBinding<GameStateData> _gameStateCompleteBinding = new();

        public bool IsInPlayMode => Application.isPlaying;

        protected void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _dynamicViews = GetComponentsInChildren<IDynamicUI>(true);
            CloseMenuGroupInstantly();
        }

        protected void OnEnable()
        {
            _gameStateStartBinding.Add(OnStartGameStateTransition);
            EventBus<GameStateData>.AddListener(_gameStateStartBinding, GameStateData.GetChannel(TransitionState.Start));

            _gameStateCompleteBinding.Add(OnCompleteGameStateTransition);
            EventBus<GameStateData>.AddListener(_gameStateCompleteBinding, GameStateData.GetChannel(TransitionState.End));
        }

        protected void OnDisable()
        {
            _gameStateStartBinding.Remove(OnStartGameStateTransition);
            EventBus<GameStateData>.RemoveListener(_gameStateStartBinding, GameStateData.GetChannel(TransitionState.Start));
            
            _gameStateCompleteBinding.Remove(OnCompleteGameStateTransition);
            EventBus<GameStateData>.RemoveListener(_gameStateCompleteBinding, GameStateData.GetChannel(TransitionState.End));
        }

        protected abstract void OnStartGameStateTransition(GameStateData gameStateData);
        protected abstract void OnCompleteGameStateTransition(GameStateData gameStateData);

        [Button, ShowIf("IsInPlayMode")]
        public void OpenMenuGroup()
        {
            _canvasGroup.blocksRaycasts = true;
            
            _canvasFadeTween.Kill();
            _canvasFadeTween = _canvasGroup.DOFade(1f, _configSo.openDuration)
                .SetEase(_configSo.ease)
                .SetDelay(_configSo.openDelay);
            
            _dynamicViews.ForEach(x => x.OpenView());
        }

        [Button, ShowIf("IsInPlayMode")]
        public void CloseMenuGroup()
        {
            _canvasGroup.blocksRaycasts = false;
            
            _canvasFadeTween?.Kill();
            _canvasFadeTween = _canvasGroup.DOFade(0f, _configSo.closeDuration)
                                            .SetEase(_configSo.ease)
                                            .SetDelay(_configSo.closeDelay);
            
            _dynamicViews.ForEach(x => x.CloseView());
        }
        private void OpenMenuGroupInstantly()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _dynamicViews.ForEach(x => x.OpenViewInstantly());
        }

        private void CloseMenuGroupInstantly()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _dynamicViews.ForEach(x => x.CloseViewInstantly());
        }
        
        protected void OnDestroy() => _canvasFadeTween?.Kill();
    }
}