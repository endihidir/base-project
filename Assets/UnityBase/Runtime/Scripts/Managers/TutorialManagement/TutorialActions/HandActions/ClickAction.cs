using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class ClickAction : HandTutorialAction
    {
        public ClickAction Click(Vector3 position, float clickDuration, Ease ease = Ease.Linear)
        {
            var pos = position.SelectSimulationSpace(_handTutorial.SpawnSpace);

            var handRect = _handTutorial.TutorialHandRect;

            _handTween.Kill();

            handRect.position = pos + Vector3.down * 100f;

            _handTween = DOTween.Sequence().Append(handRect.DORotate(Vector3.right * 30f, clickDuration).SetEase(ease))
                .AppendInterval(0.1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(_handTutorial.UseUnscaledTime);
            return this;
        }
    }
}