using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class ScaleUpUI : DynamicUI
    {
        [Title("Scale Settings")] 
        
        [SerializeField] private float _inScale = 1.1f;

        [SerializeField] private float _outScale = 1f;
        
        [SerializeField] private Ease _outScaleEase = Ease.OutBounce;
        
        [SerializeField] private float _outDuration = 0.2f;

        private Tween _scaleUp;

        [Button]
        public override void OpenUI()
        {
            base.OpenUI();

            _scaleUp.Kill();

            _scaleUp = DOTween.Sequence()
                .Append(_uiHandler.DOScale(_outScale * _inScale, _openDuration).SetEase(_openEase))
                .Append(_uiHandler.DOScale(_outScale, _outDuration).SetEase(_outScaleEase))
                .SetDelay(_openDelay).SetUpdate(UnscaledTime);

        }

        [Button]
        public override void CloseUI()
        {
            base.CloseUI();

            _scaleUp.Kill();
            _scaleUp = _uiHandler.DOScale(0f, _closeDuration)
                .SetEase(_closeEase)
                .SetDelay(_closeDelay)
                .SetUpdate(UnscaledTime);
        }

        public override void OpenUIDirectly()
        {
            base.OpenUIDirectly();

            _uiHandler.localScale = Vector3.one * _outScale;
        }

        public override void CloseUIDirectly()
        {
            base.CloseUIDirectly();

            _uiHandler.localScale = Vector3.zero;
        }

        protected override void OnDestroy()
        {
            _scaleUp?.Kill();
        }
    }
}