using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class MoveInOutAction : TextTutorialAction
    {
        public MoveInOutAction MoveInOut(Vector3 startPosition, Vector3 inPosition, Vector3 outPosition, float moveDuration, float stayDuration, Ease ease = Ease.InOutQuad)
        {
            var startPos = startPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);
            var inPos = inPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);
            var outPos = outPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);

            _textTween.Kill();

            var rect = _textTutorial.TutorialTextRect;
            rect.position = startPos;

            _textTween = DOTween.Sequence()
                .Append(rect.DOMove(inPos, moveDuration).SetEase(ease))
                .AppendInterval(stayDuration)
                .Append(rect.DOMove(outPos, moveDuration).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime)
                .AppendCallback(() => _textTutorial.Hide());

            return this;
        }

        public MoveInOutAction MoveIn(Vector3 startPosition, Vector3 inPosition, float moveDuration, Ease ease = Ease.InOutQuad)
        {
            var startPos = startPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);
            var inPos = inPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);

            _textTween.Kill();

            var rect = _textTutorial.TutorialTextRect;
            rect.position = startPos;

            _textTween = DOTween.Sequence()
                .Append(rect.DOMove(inPos, moveDuration).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime);

            return this;
        }

        public MoveInOutAction MoveOut(Vector3 outPosition, float moveDuration, Ease ease = Ease.InOutQuad)
        {
            var outPos = outPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);

            _textTween.Kill();

            var rect = _textTutorial.TutorialTextRect;

            _textTween = DOTween.Sequence()
                .Append(rect.DOMove(outPos, moveDuration).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime)
                .AppendCallback(() => _textTutorial.Hide());
            return this;
        }
    }
}