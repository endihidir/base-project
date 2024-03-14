using System;
using System.Linq;
using UnityBase.TutorialCore.TutorialAction;
using UnityEngine;

namespace UnityBase.TutorialCore
{
    public class HandTutorial : Tutorial
    {
        #region VARIABLES

        [Header("Hand Tutorial Contents")] private HandTutorialAction _selectedHandAction;

        private HandTutorialAction[] _handActions;

        [SerializeField] private RectTransform _tutorialHandRect;
        [SerializeField] private bool _useUnscaledTime;

        private Vector2 _defaultPos, _defaultSizeDelta;

        private Quaternion _defaultRot;

        private ClickAction _clickAction = new ClickAction();
        private MoveAction _moveAction = new MoveAction();
        private DrawAction _drawAction = new DrawAction();
        private SlideAction _slideAction = new SlideAction();

        #endregion

        #region PROPERTIES

        public RectTransform TutorialHandRect => _tutorialHandRect;
        public bool UseUnscaledTime => _useUnscaledTime;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            _handActions = new HandTutorialAction[] { _clickAction, _moveAction, _drawAction, _slideAction };
        }

        public T GetAction<T>(float handScaleMultiplier = 1f) where T : HandTutorialAction
        {
            _selectedHandAction = _handActions.FirstOrDefault(x => x.GetType() == typeof(T));

            _selectedHandAction?.InitDependencies(this);

            _tutorialHandRect.sizeDelta = _defaultSizeDelta * handScaleMultiplier;

            return (T)_selectedHandAction;
        }

        protected override void ResetTutorial(Action onComplete)
        {
            base.ResetTutorial(onComplete);

            _selectedHandAction?.Reset();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _selectedHandAction?.Reset();
        }

        protected override void SetDefaultTransformData()
        {
            _tutorialHandRect.position = _defaultPos;
            _tutorialHandRect.rotation = _defaultRot;
            _tutorialHandRect.sizeDelta = _defaultSizeDelta;
        }

        protected override void CashDefaultTransformData()
        {
            _defaultPos = _tutorialHandRect.position;
            _defaultRot = _tutorialHandRect.rotation;
            _defaultSizeDelta = _tutorialHandRect.sizeDelta;
        }
    }
}