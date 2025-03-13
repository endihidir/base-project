using DG.Tweening;

namespace UnityBase.TutorialCore.TutorialAction
{
    public abstract class HandTutorialAction : TutorialAction
    {
        protected HandTutorial _handTutorial;

        protected Tween _handTween;

        public virtual void InitDependencies(HandTutorial handTutorial)
        {
            _handTutorial = handTutorial;
        }

        public override void Reset()
        {
            _handTutorial = null;

            _handTween?.Kill();
        }
    }
}