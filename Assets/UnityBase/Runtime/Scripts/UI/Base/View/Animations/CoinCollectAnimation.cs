using System;
using DG.Tweening;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Runtime.Behaviours
{
    public class CoinCollectAnimation : ICoinCollectAnimation
    {
        private IPoolManager _pool;
        private CoinIconTest _prefab;
        private Transform _parent;
        private Vector3 _startPos;
        private Transform _target;
        private float _duration = 0.5f;
        private Tween _moveInOutAnim;

        private CoinIconTest _icon;

        public CoinCollectAnimation(IPoolManager pool) => _pool = pool;

        public ICoinCollectAnimation Configure(CoinIconTest prefab, Transform parent, Vector3 startPos, Transform target, float duration = 0.5f)
        {
            _prefab = prefab;
            _parent = parent;
            _startPos = startPos;
            _target = target;
            _duration = duration;
            return this;
        }

        public void Play(Action onArrive)
        {
            if (_pool == null || !_prefab || !_parent || !_target) return;

            _icon = _pool.GetObject(_prefab, false);
            _icon.Bind(_pool);
            _icon.Show(0f, 0f, null);
            _icon.transform.SetParent(_parent, worldPositionStays: false);
            _icon.transform.position = _startPos;
            _icon.MoveTo(_target, _duration, () => onArrive?.Invoke());
        }

        public void Dispose()
        {
            if (!_icon) return;
            _pool?.ReturnToPool(_icon, 0f, 0.0f);
        }
    }
    
    public interface ICoinCollectAnimation : IAnimation
    {
        ICoinCollectAnimation Configure(CoinIconTest prefab, Transform parent, Vector3 startPos, Transform target, float duration = 0.5f);
        void Play(Action onArrive);
    }
}