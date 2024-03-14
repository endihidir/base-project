using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class DrawAction : HandTutorialAction
    {
        private Tween _drawTween;

        public DrawAction Draw(Vector3[] positions, float moveDuration, PathType pathType, bool alignDirection = false, Ease ease = Ease.Linear)
        {
            var poses = SelectSimulationSpaces(positions);

            _drawTween.Kill();

            var handRect = _handTutorial.TutorialHandRect;

            handRect.position = poses[0];

            var duration = moveDuration * (poses.Length - 1);

            if (alignDirection)
            {
                _drawTween = handRect.DOPath(poses, duration, pathType)
                    .SetLookAt(0.01f, Vector3.left)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(ease).SetUpdate(_handTutorial.UseUnscaledTime);

            }
            else
            {
                _drawTween = handRect.DOPath(poses, duration, pathType)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(ease).SetUpdate(_handTutorial.UseUnscaledTime);
            }

            return this;
        }

        private Vector3[] SelectSimulationSpaces(Vector3[] positions)
        {
            var poses = positions;

            for (int i = 0; i < positions.Length; i++)
                poses[i] = positions[i].SelectSimulationSpace(_handTutorial.SpawnSpace);

            return poses;
        }

        public override void Reset()
        {
            base.Reset();

            _drawTween?.Kill();
        }
    }
}