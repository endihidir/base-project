using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class ScaleAction : TextTutorialAction
    {
        public ScaleAction ScaleUp(Vector3 startPosition, float scaleDuration, float delay, Ease ease = Ease.InOutQuad)
        {
            var startPos = startPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);

            var rect = _textTutorial.TutorialTextRect;

            _textTween.Kill();

            rect.localScale = Vector3.zero;

            rect.position = startPos;

            _textTween = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(rect.DOScale(1f, scaleDuration).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime);

            return this;
        }

        public ScaleAction ScaleDown(float scaleDuration, float delay, Ease ease = Ease.InOutQuad)
        {
            var rect = _textTutorial.TutorialTextRect;

            _textTween.Kill();

            rect.localScale = Vector3.one;

            _textTween = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(rect.DOScale(0f, scaleDuration).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime)
                .AppendCallback(() => _textTutorial.Hide());
            return this;
        }
    }
}