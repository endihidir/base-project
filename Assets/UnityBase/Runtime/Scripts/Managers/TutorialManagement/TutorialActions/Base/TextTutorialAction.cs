using DG.Tweening;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class TextTutorialAction : TutorialAction
    {
        protected TextTutorial _textTutorial;

        protected Tween _textTween;

        public virtual void InitDependencies(TextTutorial textTutorial)
        {
            _textTutorial = textTutorial;
        }

        public override void Reset()
        {
            _textTween?.Kill();

            _textTutorial = null;
        }

        public virtual TextTutorialAction SetText(string text)
        {
            _textTutorial.TextUI.SetText(text);
            return this;
        }
    }
}