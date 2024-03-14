using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class MoveAction : HandTutorialAction
    {
        private Vector3 _startPosition, _endPosition;

        public MoveAction Move(Vector3 startPosition, Vector3 endPosition, float moveDuration, float clickDuration, Ease ease = Ease.Linear)
        {
            _startPosition = startPosition.SelectSimulationSpace(_handTutorial.SpawnSpace);
            _endPosition = endPosition.SelectSimulationSpace(_handTutorial.SpawnSpace);

            var handRect = _handTutorial.TutorialHandRect;

            _handTween.Kill();

            handRect.position = _startPosition;

            _handTween = DOTween.Sequence()
                .AppendInterval(0.1f)
                .Append(handRect.DORotate(Vector3.right * 30f, clickDuration, RotateMode.LocalAxisAdd).SetEase(ease))
                .AppendInterval(0.15f)
                .Append(handRect.DOMove(_endPosition, moveDuration).SetEase(ease))
                .Append(handRect.DORotate(Vector3.left * 30f, clickDuration, RotateMode.LocalAxisAdd).SetEase(ease))
                .AppendInterval(0.15f)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(_handTutorial.UseUnscaledTime);
            return this;
        }

        public void AlignDirection(bool reverseDir = false)
        {
            var handRect = _handTutorial.TutorialHandRect;

            var startPos = reverseDir ? _endPosition : _startPosition;

            var endPos = reverseDir ? _startPosition : _endPosition;

            var lookRot = handRect.rotation.LookAt2D(startPos, endPos);

            handRect.rotation = lookRot;
        }
    }
}