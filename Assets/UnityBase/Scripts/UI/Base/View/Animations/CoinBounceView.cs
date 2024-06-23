using System;
using DG.Tweening;
using UnityBase.UI.Config.SO;
using UnityEngine;

namespace UnityBase.UI.ViewCore
{
    public class BounceView : IBounceView
    {
        private Transform _coinIconTransform;

        private BounceViewConfigSO _bounceViewConfig;

        private Tween _bounceTween;

        public Transform CoinIconTransform => _coinIconTransform;
        
        public IBounceView Initialize(Transform coinIconTransform)
        {
            _coinIconTransform = coinIconTransform;
            return this;
        }

        public IBounceView Configure(BounceViewConfigSO bounceViewConfigSo)
        {
            _bounceViewConfig = bounceViewConfigSo;
            return this;
        }

        public void Bounce(Action onComplete)
        {
            _bounceTween?.Kill(true);
            
            _bounceTween = _coinIconTransform.DOPunchScale(Vector3.one * _bounceViewConfig.scaleMultiplier, _bounceViewConfig.duration)
                                             .SetEase(_bounceViewConfig.ease)
                                             .SetUpdate(_bounceViewConfig.useUnscaledTime)
                                             .OnComplete(()=>
                                             {
                                                 _coinIconTransform.localScale = Vector3.one;
                                                 onComplete?.Invoke();
                                             });
        }
        
        public void Dispose() => _bounceTween?.Kill();
    }
    
    public interface IBounceView : IViewAnimation
    {
        public IBounceView Initialize(Transform coinIconTransform);
        public IBounceView Configure(BounceViewConfigSO bounceViewConfigSo);
        public void Bounce(Action onComplete);
    }
}