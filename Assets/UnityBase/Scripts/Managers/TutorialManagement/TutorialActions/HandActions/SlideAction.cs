using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class SlideAction : HandTutorialAction
    {
        private Vector3 _startPosition, _endPosition;

        public SlideAction Slide(Vector3 startPosition, Vector3 endPosition, float moveDuration, Ease ease = Ease.Linear)
        {
            _startPosition = startPosition.SelectSimulationSpace(_handTutorial.SpawnSpace);
            _endPosition = endPosition.SelectSimulationSpace(_handTutorial.SpawnSpace);

            var handRect = _handTutorial.TutorialHandRect;

            _handTween.Kill();

            handRect.position = _startPosition;

            _handTween = DOTween.Sequence()
                .AppendInterval(0.1f)
                .Append(handRect.DOMove(_endPosition, moveDuration).SetEase(ease))
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(_handTutorial.UseUnscaledTime);
            return this;
        }

        public void AlignDirection()
        {
            var handRect = _handTutorial.TutorialHandRect;

            var lookRot = handRect.rotation.LookAt2D(_startPosition, _endPosition);

            handRect.rotation = lookRot;
        }
    }
}