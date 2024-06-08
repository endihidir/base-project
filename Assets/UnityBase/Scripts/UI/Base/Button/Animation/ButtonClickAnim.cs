using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonClickAnim : ButtonAnimation
    {
        private Transform _buttonTransform;
        
        private Tween _bounceTween;

        private float _startValue, _endValue, _duration;

        private Ease _ease;

        public ButtonClickAnim(IButtonUI buttonUI) : base(buttonUI)
        {
            _buttonTransform = _buttonUI.Transform;
        }
        
        public void Configure(float startValue, float endValue, float duration, Ease ease)
        {
            _startValue = startValue;
            _endValue = endValue;
            _duration = duration;
            _ease = ease;
        }

        public override async UniTask Click()
        {
            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override async UniTask PointerDown()
        {
            _bounceTween?.Kill();

            _bounceTween = _buttonTransform.DOScale(_startValue, _duration).SetEase(_ease);

            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override async UniTask PointerUp()
        {
            _bounceTween?.Kill();

            _bounceTween = _buttonTransform.DOScale(_endValue, _duration).SetEase(_ease);

            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override void Dispose() => _bounceTween?.Kill();
    }
}