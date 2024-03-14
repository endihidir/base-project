using System;
using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.TutorialCore.TutorialAction
{
    public class TypeWriterAction : TextTutorialAction
    {
        private string _defaultText = "";
        private event Action _onActionComplete;

        public TypeWriterAction TypeWriter(string fullText, Vector3 startPosition, float delay, Ease ease = Ease.Linear)
        {
            var startPos = startPosition.SelectSimulationSpace(_textTutorial.SpawnSpace);

            _textTween.Kill();

            var rect = _textTutorial.TutorialTextRect;
            rect.position = startPos;
            _textTutorial.TextUI.text = _defaultText;

            _textTween = DOTween.Sequence().Append(DOTween.To(GetTextLength, x => UpdateText(fullText, x), fullText.Length, 
                    fullText.Length * delay).SetEase(ease))
                .SetUpdate(_textTutorial.UseUnscaledTime)
                .AppendCallback(() => _onActionComplete?.Invoke());
            return this;
        }

        private int GetTextLength()
        {
            return _defaultText.Length;
        }

        private void UpdateText(string fullText, int index)
        {
            _defaultText = fullText.Substring(0, index);
            _textTutorial.TextUI.text = _defaultText;
        }

        public override void Reset()
        {
            base.Reset();

            _defaultText = "";
        }

        public TypeWriterAction OnActionComplete(Action act)
        {
            _onActionComplete = act;
            return this;
        }
    }
}