using System;
using System.Linq;
using TMPro;
using UnityBase.TutorialCore.TutorialAction;
using UnityEngine;

namespace UnityBase.TutorialCore
{
    public class TextTutorial : Tutorial
    {
        #region VARIABLES

        [Header("Text Tutorial Contents")] private TextTutorialAction _selectedTextAction;

        private TextTutorialAction[] _textActions;

        [SerializeField] private RectTransform _tutorialTextRect;

        [SerializeField] private TextMeshProUGUI _textUI;

        [SerializeField] private bool _useUnscaledTime;

        private Vector2 _defaultPos, _defaultSizeDelta;

        private Quaternion _defaultRot;

        private ScaleAction _scaleAction = new ScaleAction();
        private TypeWriterAction _typeWriterAction = new TypeWriterAction();
        private MoveInOutAction _moveInOutAction = new MoveInOutAction();

        #endregion

        #region PROPERTIES

        public RectTransform TutorialTextRect => _tutorialTextRect;
        public TextMeshProUGUI TextUI => _textUI;

        public Vector2 DefaultSizeDelta => _defaultSizeDelta;
        public bool UseUnscaledTime => _useUnscaledTime;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            _textActions = new TextTutorialAction[] { _scaleAction, _typeWriterAction, _moveInOutAction };
        }

        public T Action<T>(Vector2 sizeDelta, int fontSize = 36) where T : TextTutorialAction
        {
            _selectedTextAction = _textActions.FirstOrDefault(x => x.GetType() == typeof(T));

            _selectedTextAction?.InitDependencies(this);

            _tutorialTextRect.sizeDelta = sizeDelta;

            _textUI.fontSize = fontSize;

            return (T)_selectedTextAction;
        }

        protected override void ResetTutorial(Action onComplete)
        {
            base.ResetTutorial(onComplete);

            _selectedTextAction?.Reset();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _selectedTextAction?.Reset();
        }

        protected override void SetDefaultTransformData()
        {
            _tutorialTextRect.position = _defaultPos;
            _tutorialTextRect.rotation = _defaultRot;
            _tutorialTextRect.sizeDelta = _defaultSizeDelta;
            _tutorialTextRect.localScale = Vector3.one;
            _textUI.text = "";
        }

        protected override void CashDefaultTransformData()
        {
            _defaultPos = _tutorialTextRect.position;
            _defaultRot = _tutorialTextRect.rotation;
            _defaultSizeDelta = _tutorialTextRect.sizeDelta;
        }
    }
}