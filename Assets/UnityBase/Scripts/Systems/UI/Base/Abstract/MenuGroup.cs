using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityBase.EventBus;
using UnityBase.Manager.Data;
using UnityBase.UI.Base;
using UnityBase.UI.Dynamic;
using UnityEngine;

namespace UnityBase.UI.Menu
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuGroup : BaseUIElements
    {
        [Title("General Settings")] 
        [SerializeField] protected bool _openOnStart;

        [SerializeField] protected float _openDuration = 0.5f, _cloaseDuration = 0.5f;

        [SerializeField] protected float _openDelay, _closeDelay;

        [SerializeField] protected Ease _ease = Ease.InOutQuad;

        [SerializeField] protected bool _unscaledTime;

        [Title("ReadOnly Settings")] 
        [ReadOnly] [SerializeField] protected CanvasGroup _canvasGroup;

        [ReadOnly] [SerializeField] protected DynamicUI[] _dynamicUIs;

        private Tween _canvasFade;

        private EventBinding<GameStateData> _gameStateStartBinding = new EventBinding<GameStateData>();
        private EventBinding<GameStateData> _gameStateCompleteBinding = new EventBinding<GameStateData>();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _canvasGroup = GetComponent<CanvasGroup>();

            _dynamicUIs = GetComponentsInChildren<DynamicUI>(true);

            for (int i = 0; i < _dynamicUIs.Length; i++)
            {
                _dynamicUIs[i].UnscaledTime = _unscaledTime;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            if (_openOnStart)
            {
                OpenAllDependenciesDirectly();
            }
            else
            {
                CloseAllDependenciesDirectly();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _gameStateStartBinding.Add(OnStartGameStateTransition);

            EventBus<GameStateData>.AddListener(_gameStateStartBinding, GameStateData.GetChannel(TransitionState.Start));

            _gameStateCompleteBinding.Add(OnCompleteGameStateTransition);

            EventBus<GameStateData>.AddListener(_gameStateCompleteBinding, GameStateData.GetChannel(TransitionState.End));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _gameStateStartBinding.Remove(OnStartGameStateTransition);

            EventBus<GameStateData>.RemoveListener(_gameStateStartBinding, GameStateData.GetChannel(TransitionState.Start));
            
            _gameStateCompleteBinding.Remove(OnCompleteGameStateTransition);

            EventBus<GameStateData>.RemoveListener(_gameStateCompleteBinding, GameStateData.GetChannel(TransitionState.End));
        }

        protected abstract void OnStartGameStateTransition(GameStateData gameStateData);
        protected abstract void OnCompleteGameStateTransition(GameStateData gameStateData);

        [Button]
        public void Open()
        {
            OpenAllDependencies();
        }

        [Button]
        public void Close()
        {
            CloseAllDependencies();
        }

        protected void OpenAllDependencies()
        {
            OpenCanvasGroup();

            _dynamicUIs.ForEach(x => x.OpenUI());
        }

        protected void CloseAllDependencies()
        {
            CloseCanvasGroup();

            _dynamicUIs.ForEach(x => x.CloseUI());
        }

        private void OpenAllDependenciesDirectly()
        {
            OpenCanvasGroupDirectly();

            _dynamicUIs.ForEach(x => x.OpenUIDirectly());
        }

        private void CloseAllDependenciesDirectly()
        {
            CloseCanvasGroupDirectly();

            _dynamicUIs.ForEach(x => x.CloseUIDirectly());
        }

        private void OpenCanvasGroup()
        {
            _canvasGroup.interactable = true;

            _canvasFade.Kill();
            _canvasFade = _canvasGroup.DOFade(1f, _openDuration)
                .SetEase(_ease)
                .SetDelay(_openDelay)
                .SetUpdate(_unscaledTime)
                .OnComplete(() => _canvasGroup.blocksRaycasts = true);
        }

        private void CloseCanvasGroup()
        {
            _canvasGroup.blocksRaycasts = false;

            _canvasFade.Kill();
            _canvasFade = _canvasGroup.DOFade(0f, _cloaseDuration)
                .SetEase(_ease)
                .SetDelay(_closeDelay)
                .SetUpdate(_unscaledTime)
                .OnComplete(() => _canvasGroup.interactable = false);
        }

        private void OpenCanvasGroupDirectly()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void CloseCanvasGroupDirectly()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        protected override void OnDestroy()
        {
            _canvasFade?.Kill();
        }
    }
}