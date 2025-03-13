using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonClickAnim : ButtonAnimation
    {
        private Transform _buttonTransform;
        
        private Tween _onClickTween;

        private float _startScale, _duration;

        private bool _isPointerUp;

        private Ease _ease;

        public ButtonClickAnim(IButtonUI buttonUI) : base(buttonUI)
        {
            _buttonTransform = _buttonUI.Button.transform;
        }
        
        public IButtonAnimation Configure(float scaleUpValue, float duration, Ease ease)
        {
            _startScale = scaleUpValue;
            _duration = duration;
            _ease = ease;
            return this;
        }

        public override async UniTask Click()
        {
            await UniTask.WaitUntil(()=> _isPointerUp);
        }

        public override async UniTask PointerDown()
        {
            _onClickTween?.Kill();

            _isPointerUp = false;

            _onClickTween = _buttonTransform.DOScale(_startScale, _duration).SetEase(_ease);

            await _onClickTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override async UniTask PointerUp()
        {
            _onClickTween?.Kill();

            _onClickTween = _buttonTransform.DOScale(1f, _duration).SetEase(_ease);

            await _onClickTween.AsyncWaitForCompletion().AsUniTask();

            _isPointerUp = true;
        }

        public override void Dispose() => _onClickTween?.Kill();
    }
}