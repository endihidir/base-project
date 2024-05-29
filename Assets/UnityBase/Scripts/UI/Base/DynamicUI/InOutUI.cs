using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class InOutUI : DynamicUI
    {
        [Title("Position Settings")] 
        [SerializeField] protected bool _useBounceEffect;

        [ShowIf("_useBounceEffect")] 
        [SerializeField] protected float _bounceMultiplier = 1f;

        [ShowIf("_useBounceEffect")] 
        [SerializeField] protected float _bounceDuration = 0.15f;

        [ShowIf("_useBounceEffect")] 
        [SerializeField] protected Ease _bounceEase = Ease.OutBounce;

        [ShowIf("_useBounceEffect")] 
        [SerializeField] protected float _bounceDelayMultiplier = 0.9f;

        [SerializeField] protected Vector2 _inPos, _outPos;

        private Tween _moveTween, _bounceTween;

        [Button]
        public override void OpenUI()
        {
            base.OpenUI();

            _moveTween.Kill();
            _moveTween = _rectTransform.DOAnchorPos(_inPos, _openDuration)
                .SetEase(_openEase)
                .SetDelay(_openDelay)
                .SetUpdate(UnscaledTime);

            var delay = (_openDuration + _openDelay) * _bounceDelayMultiplier;

            BounceEffect(delay);
        }

        [Button]
        public override void CloseUI()
        {
            base.CloseUI();

            _moveTween.Kill();
            _moveTween = _rectTransform.DOAnchorPos(_outPos, _closeDuration)
                .SetEase(_closeEase)
                .SetDelay(_closeDelay)
                .SetUpdate(UnscaledTime);
        }

        private void BounceEffect(float delay)
        {
            if (!_useBounceEffect) return;
            
            _bounceTween.Kill(true);

            var scaleVal = _bounceMultiplier * 0.1f;
            var bounceScale = new Vector3(1f - scaleVal, 1f + scaleVal, 1f);

            _bounceTween = _uiHandler.DOScale(bounceScale, _bounceDuration).SetEase(_bounceEase)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => _uiHandler.localScale = Vector3.one).SetDelay(delay);
        }

        public override void OpenUIDirectly()
        {
            base.OpenUIDirectly();

            _rectTransform.anchoredPosition = _inPos;
        }

        public override void CloseUIDirectly()
        {
            base.CloseUIDirectly();

            _rectTransform.anchoredPosition = _outPos;
        }

        protected override void OnDestroy()
        {
            _moveTween?.Kill();
            _bounceTween?.Kill();
        }
    }
}