using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.ButtonCore
{
    public class UpDownBounceUIAnimation : UIAnimation
    {
        private Transform _buttonTransform;
        
        private Tween _bounceTween;
        
        public UpDownBounceUIAnimation(IButtonUI buttonUI) : base(buttonUI)
        {
            _buttonTransform = _buttonUI.Transform;
        }

        public override UniTask OpenAnimation()
        {
            return default;
        }

        public override UniTask CloseAnimation()
        {
            return default;
        }

        public override async UniTask ClickAnimation()
        {
            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override async UniTask PointerDownAnimation()
        {
            _bounceTween?.Kill();

            _bounceTween = _buttonTransform.DOScale(1.05f, 0.1f).SetEase(Ease.InOutQuad);

            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override async UniTask PointerUpAnimation()
        {
            _bounceTween?.Kill();

            _bounceTween = _buttonTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad);

            await _bounceTween.AsyncWaitForCompletion().AsUniTask();
        }

        public override void Dispose() => _bounceTween?.Kill();
    }
}