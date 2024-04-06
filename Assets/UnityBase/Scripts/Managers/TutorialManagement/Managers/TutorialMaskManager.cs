using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBase.Extensions;
using UnityBase.ManagerSO;
using UnityBase.Pool;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.Manager
{
    public class TutorialMaskManager : ITutorialMaskDataService, IAppPresenterDataService
    {
        private GameObject _maskRoot;

        private Transform _maskUIPool;

        private Image _maskFadePanel;

        private List<MaskUIRaycastFilter> _maskRaycastFilters = new List<MaskUIRaycastFilter>();

        private CancellationTokenSource _delayCancellationToken;

        private readonly IPoolDataService _poolDataService;

        public TutorialMaskManager(ManagerDataHolderSO managerDataHolderSo, IPoolDataService poolDataService)
        {
            var maskManagerSo = managerDataHolderSo.tutorialMaskManagerSo;

            _maskRoot = maskManagerSo.maskRoot;

            _maskUIPool = maskManagerSo.maskUIPool;

            _maskFadePanel = maskManagerSo.maskFadePanel;

            _poolDataService = poolDataService;
        }

        ~TutorialMaskManager() => Dispose();
        
        public void Initialize() { }
        public void Start() => SetFadePanelsOpacity();
        public void Dispose() => DisposeToken();

        // Note : If masks spawn on the same position, they don't appear !!!
        public MaskUI GetMask(Vector3 position, MaskUIData maskUIData, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default)
        {
            var selectedMask = _poolDataService.GetObject<MaskUI>(show, duration, delay, onComplete);

            PrepareMask(selectedMask, position, maskUIData);

            SetFadePanelColor(maskUIData.fadePanelOpacity);

            return selectedMask;
        }

        public bool TryGetMask(Vector3 position, MaskUIData maskUIData, out MaskUI maskUI, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false)
        {
            maskUI = default;

            var poolCount = _poolDataService.GetPoolCount<MaskUI>(readLogs);

            if (poolCount < 1) return false;

            maskUI = _poolDataService.GetObject<MaskUI>(show, duration, delay, onComplete);

            PrepareMask(maskUI, position, maskUIData);

            SetFadePanelColor(maskUIData.fadePanelOpacity);

            return true;
        }

        public MaskUI[] GetMasks(Vector3[] positions, MaskUIData maskUIData, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default)
        {
            var masks = new MaskUI[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                var selectedMask = _poolDataService.GetObject<MaskUI>(show, duration, delay, onComplete);

                PrepareMask(selectedMask, positions[i], maskUIData);

                masks[i] = selectedMask;
            }

            if (positions.Length > 0) SetFadePanelColor(maskUIData.fadePanelOpacity);

            return masks;
        }

        public bool TryGetMasks(Vector3[] positions, MaskUIData maskUIData, out MaskUI[] masks, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false)
        {
            masks = new MaskUI[positions.Length];

            var poolCount = _poolDataService.GetPoolCount<MaskUI>(readLogs);

            if (poolCount < positions.Length) return false;

            for (int i = 0; i < positions.Length; i++)
            {
                var selectedMask = _poolDataService.GetObject<MaskUI>(show, duration, delay, onComplete);

                var position = positions[i];

                PrepareMask(selectedMask, position, maskUIData);

                masks[i] = selectedMask;
            }

            if (positions.Length > 0) SetFadePanelColor(maskUIData.fadePanelOpacity);

            return true;
        }

        public async void HideMask(MaskUI maskUI, float killDuration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false)
        {
            _poolDataService.HideObject(maskUI, killDuration, delay, default, readLogs);

            var raycastFitter = _maskRaycastFilters.FirstOrDefault(x => x.TargetMaskUI == maskUI);

            if (raycastFitter)
            {
                raycastFitter.TargetMaskUI = null;
            }

            await DeactivateFadePanelAsync(killDuration, delay);
        }

        public async void HideAllMasks(float killDuration = 0f, float delay = 0f)
        {
            _poolDataService.HideAllObjectsOfType<MaskUI>(killDuration, delay);

            _maskRaycastFilters.ForEach(x => x.TargetMaskUI = null);

            await DeactivateFadePanelAsync(killDuration, delay);
        }
        
        public void RemoveMaskPool<T>(bool readLogs = false) where T : MaskUI => _poolDataService.RemovePool<T>(readLogs);

        //---------------------------------------------------------------------------------------------------------------
        
        private void PrepareMask(MaskUI selectedMaskUI, Vector3 position, MaskUIData maskUIData)
        {
            selectedMaskUI.transform.SetParent(_maskUIPool);

            InitRaycastFitter(selectedMaskUI);

            var scale = maskUIData.scale;
            var smallerSideSize = scale.x < scale.y ? scale.x : scale.y;
            var pixelSize = (1 / smallerSideSize) * selectedMaskUI.Image.sprite.rect.width;

            var positionSpace = maskUIData.positionSpace;
            var cornerSharpnessMultiplier = maskUIData.cornerSharpnessMultiplier;
            var fadePanelOpacity = maskUIData.fadePanelOpacity;

            selectedMaskUI.PrepareMask(position.SelectSimulationSpace(positionSpace), scale, pixelSize, cornerSharpnessMultiplier);
            selectedMaskUI.SetMaskPanelStartColor(_maskFadePanel.color.SetAlpha(fadePanelOpacity));
        }

        private void SetFadePanelColor(float fadePanelOpacity)
        {
            _delayCancellationToken?.Cancel();
            _maskFadePanel.gameObject.SetActive(true);
            _maskFadePanel.color = _maskFadePanel.color.SetAlpha(fadePanelOpacity);
        }

        private async UniTask DeactivateFadePanelAsync(float duration, float delay)
        {
            CancellationTokenExtentions.Refresh(ref _delayCancellationToken);

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration + delay), DelayType.DeltaTime, PlayerLoopTiming.Update, _delayCancellationToken.Token);

                _maskFadePanel.gameObject.SetActive(false);
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e);
            }
        }

        private void InitRaycastFitter(MaskUI selectedMaskUI)
        {
            var raycastFitter = _maskRaycastFilters.FirstOrDefault(x => x.TargetMaskUI is null);

            if (!raycastFitter)
            {
                var newRaycastFitter = _maskRoot.AddComponent<MaskUIRaycastFilter>();
                newRaycastFitter.TargetMaskUI = selectedMaskUI;
                _maskRaycastFilters.Add(newRaycastFitter);
            }
            else
            {
                raycastFitter.TargetMaskUI = selectedMaskUI;
            }
        }

        private void SetFadePanelsOpacity()
        {
            var activePoolables = PoolableObjectGroup.FindPoolablesOfType<MaskUI>();

            foreach (var activePoolable in activePoolables)
            {
                activePoolable.SetMaskPanelStartColor(_maskFadePanel.color);
            }
        }

        private void DisposeToken()
        {
            _delayCancellationToken?.Cancel();
            _delayCancellationToken?.Dispose();
        }
    }
}