using DG.Tweening;
using UnityBase.UI.Config.SO;
using UnityEngine;

namespace UnityBase.UI.ViewCore
{
    public class MoveInOutView : IMoveInOutView
    {
        private RectTransform _mainTransform;

        private Tween _moveTween;
        
        private MoveInOutViewConfigSO _moveInOutViewConfigSo;
        
        public IMoveInOutView Initialize(RectTransform mainTransform)
        {
            _mainTransform = mainTransform;
            return this;
        }

        public IMoveInOutView Configure(MoveInOutViewConfigSO moveInOutViewConfig)
        {
            _moveInOutViewConfigSo = moveInOutViewConfig;
            return this;
        }

        public void MoveIn()
        {
            _moveTween?.Kill();
            
            _moveTween = _mainTransform.DOAnchorPos(_moveInOutViewConfigSo.inPos, _moveInOutViewConfigSo.openDuration)
                                       .SetDelay(_moveInOutViewConfigSo.openDelay)
                                       .SetEase(_moveInOutViewConfigSo.openEase)
                                       .SetUpdate(_moveInOutViewConfigSo.useUnscaledTime);
        }

        public void MoveOut()
        {
            _moveTween?.Kill();
            
            _moveTween = _mainTransform.DOAnchorPos(_moveInOutViewConfigSo.outPos, _moveInOutViewConfigSo.closeDuration)
                                       .SetDelay(_moveInOutViewConfigSo.closeDelay)
                                       .SetEase(_moveInOutViewConfigSo.closeEase)
                                       .SetUpdate(_moveInOutViewConfigSo.useUnscaledTime);
        }

        public void MoveInInstantly()
        {
            _moveTween?.Kill();

            _mainTransform.anchoredPosition = _moveInOutViewConfigSo.inPos;
        }

        public void MoveOutInstantly()
        {
            _moveTween?.Kill();

            _mainTransform.anchoredPosition = _moveInOutViewConfigSo.outPos;
        }

        public void Dispose()
        {
            _moveTween?.Kill();
        }
    }

    public interface IMoveInOutView : IViewAnimation
    {
        public IMoveInOutView Initialize(RectTransform mainTransform);
        public IMoveInOutView Configure(MoveInOutViewConfigSO moveInOutViewConfig);
        public void MoveIn();
        public void MoveOut();
        public void MoveInInstantly();
        public void MoveOutInstantly();
    }

}