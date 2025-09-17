using System;
using DG.Tweening;
using UnityEngine;

namespace __Funflare.Scripts.Missions
{
    public class StarUIObject : MonoBehaviour, IStarUI
    {
        [SerializeField] private RectTransform _starHandler;
        
        private RectTransform _rectTransform;

        private Vector2 _endPos, _endScale;
        
        private Tween _anim, _rotationAnim;
        private float _delay;
        public Transform Transform => transform;
        public RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

        public void Initialize(Vector2 startPos, Vector2 endPos, Vector2 startScale, Vector2 endScale, bool useRotation, float delay)
        {
            Transform.position = startPos;
            Transform.localScale = startScale;

            _delay = delay;

            _endPos = endPos;
            _endScale = endScale;

            if (useRotation)
            {
                InitRotationAnim();
            }
            
            gameObject.SetActive(false);
        }

        public Tween Move(float duration, Ease ease, Action onComplete)
        {
            _anim = DOTween.Sequence()
                .AppendInterval(_delay)
                .AppendCallback(OnMovementStart)
                .Append(Transform.DOMove(_endPos, duration))
                .Join(Transform.DOScale(_endScale, duration))
                .SetEase(ease)
                .SetUpdate(true)
                .OnComplete(()=> OnMoveComplete(onComplete));

            return _anim;
        }

        public Tween Move(float duration, float curveDistance, CurveSide curveSide, Action onComplete)
        {
            var startPos = new Vector2(Transform.position.x, Transform.position.y);
            var centerPos = (_endPos + startPos) * 0.5f;
            var direction = _endPos - startPos;
            var normalUp = new Vector2(-direction.y, direction.x).normalized;
            var normalDown = new Vector2(direction.y, -direction.x).normalized;
            var centerDir = curveSide == CurveSide.Up ? normalUp : normalDown;
            var curvedCenterPos = centerPos + (centerDir * curveDistance);
            
            var middlePos = new Vector3(curvedCenterPos.x, curvedCenterPos.y - curveDistance * 2f);
            
            _anim = DOTween.Sequence()
                .AppendInterval(_delay)
                .AppendCallback(OnMovementStart)
                .Append(Transform.DOMove(middlePos, duration * 0.4f).SetEase(Ease.OutCubic))
                .Append(Transform.DOMove(_endPos, duration * 0.6f).SetEase(Ease.InCirc))
                .Join(Transform.DOScale(_endScale, duration * 0.61f).SetEase(Ease.InBack))
                .OnComplete(()=> OnMoveComplete(onComplete))
                .SetUpdate(true);
            
            return _anim;
        }

        private void OnMoveComplete(Action onComplete)
        {
            _rotationAnim?.Pause();
            
            onComplete?.Invoke();
        }
        
        private void InitRotationAnim()
        {
            _rotationAnim = _starHandler.DORotate(new Vector3(0f, 0f, 360f), 0.75f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLoops(-1, LoopType.Incremental)
                .OnPause(()=> _starHandler.rotation = Quaternion.identity);
            _rotationAnim.Pause();
        }

        private void OnMovementStart()
        {
            gameObject.SetActive(true);
            _rotationAnim?.Play();
        }

        public void SetPosition(Vector2 pos) => Transform.position = pos;
        private void OnDestroy() => _anim?.Kill();
    }
    
    public interface IStarUI
    {
        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
        public void Initialize(Vector2 startPos, Vector2 endPos, Vector2 startScale, Vector2 endScale, bool useRotation, float delay);
        public Tween Move(float duration, Ease ease, Action onComplete);
        public Tween Move(float duration, float curveDistance, CurveSide curveSide, Action onComplete);
        public void SetPosition(Vector2 pos);
    }

    public enum CurveSide
    {
        Up,
        Down
    }
}