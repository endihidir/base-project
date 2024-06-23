using System;
using System.Collections.Generic;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class TutorialProcessManager : ITutorialProcessManager, IAppBootService
    {
        private readonly ILevelManager _levelManager;
        private readonly ITutorialActionManager _tutorialActionManager;
        private readonly ITutorialMaskManager _tutorialMaskManager;
        private readonly IJsonDataManager _jsonDataManager;
        
        private const string TUTORIAL_STEP_KEY = "TutorialStepKey";
        private const string COMPLETED_TURORIAL_LEVEL_KEY = "CompletedTutorialLevelKey";

        public static Action OnCompleteTutorialStep;
        public static Action<TutorialSubStep> OnUpdateTutorialSubStep;

        private TutorialStepManagerSO _tutorialStepManagerSo;

        private bool _disableTutorial;
        private TutorialSubStepData[] _tutorialStepData;
        private TutorialStep _currentTutorialStep;
        private int _tutorialSubStepIndex = 0;
        private TutorialSubStep _currentTutorialSubStep;

        private CompletedTutorialLevelData _completedTutorialLevelData;

        private int TutorialStepIndex
        {
            get => PlayerPrefs.GetInt(TUTORIAL_STEP_KEY, (int)_currentTutorialStep);
            set => PlayerPrefs.SetInt(TUTORIAL_STEP_KEY, value);
        }

        public bool IsUnlockedLevelTutorialEnabled => IsLevelMatchedWithTutorial(_levelManager.LastUnlockedChapterIndex, _levelManager.LastUnlockedLevelIndex)
                                                      && !IsUnlockedLevelTutorialCompleted();

        public bool IsSelectedLevelTutorialEnabled => IsLevelMatchedWithTutorial(_levelManager.LastSelectedChapterIndex, _levelManager.LastSelectedLevelIndex)
                                                      && !IsSelectedLevelTutorialCompleted();

        public TutorialProcessManager(GameDataHolderSO gameDataHolderSo, ILevelManager levelManager, ITutorialActionManager tutorialActionManager, ITutorialMaskManager tutorialMaskManager, IJsonDataManager jsonDataManager)
        {
            _tutorialStepManagerSo = gameDataHolderSo.tutorialStepManagerSo;

            _disableTutorial = _tutorialStepManagerSo.disableTutorial;
            _tutorialStepData = _tutorialStepManagerSo.tutorialStepData;
            _currentTutorialStep = _tutorialStepManagerSo.currentTutorialStep;
            _tutorialSubStepIndex = _tutorialStepManagerSo.tutorialSubStepIndex;
            _currentTutorialSubStep = _tutorialStepManagerSo.currentTutorialSubStep;

            _levelManager = levelManager;
            _tutorialActionManager = tutorialActionManager;
            _tutorialMaskManager = tutorialMaskManager;
            
            _jsonDataManager = jsonDataManager;
        }

        ~TutorialProcessManager() => Dispose();

        public void Initialize()
        {
            _completedTutorialLevelData = GetCompletedTutorialLevelIndexes();

            _currentTutorialStep = (TutorialStep)TutorialStepIndex;

            _tutorialStepManagerSo.currentTutorialStep = _currentTutorialStep;
        }

        public void Dispose()
        {
            OnCompleteTutorialStep -= PasstoNextTutorialStep;
        }

        private void PasstoNextTutorialStep()
        {
            if (IsTutorialStepDataEmpty()) return;

            _tutorialActionManager.HideAllTutorials();
            _tutorialMaskManager.HideAllMasks();

            UpdateTutorialSubSteps();
        }

        private void UpdateTutorialSubSteps()
        {
            _tutorialSubStepIndex++;
            _tutorialStepManagerSo.tutorialSubStepIndex = _tutorialSubStepIndex;

            _currentTutorialSubStep = GetCurrentTutorialSubStep(_tutorialSubStepIndex);
            _tutorialStepManagerSo.currentTutorialSubStep = _currentTutorialSubStep;

            OnUpdateTutorialSubStep?.Invoke(_currentTutorialSubStep);

            if (_currentTutorialSubStep == TutorialSubStep.Completed)
            {
                _tutorialSubStepIndex = 0;
                _tutorialStepManagerSo.tutorialSubStepIndex = _tutorialSubStepIndex;

                _completedTutorialLevelData.indexes.Add(_levelManager.LastUnlockedLevelIndex);

                SetCompletedTutorialLevelIndexes(_completedTutorialLevelData);

                UpdateTutorialStep();
            }
        }

        public TutorialSubStep GetCurrentTutorialSubStep(int index)
        {
            var stepData = GetCurrentTutorialStep();

            return stepData.tutorialSubSteps[index];
        }

        private void UpdateTutorialStep()
        {
            if (TutorialStepIndex < _tutorialStepData.Length - 1)
            {
                TutorialStepIndex++;

                _currentTutorialStep = (TutorialStep)TutorialStepIndex;

                _tutorialStepManagerSo.currentTutorialStep = _currentTutorialStep;
            }
        }

        private bool IsLevelMatchedWithTutorial(int chapterIndex, int levelIndex)
        {
            if (IsTutorialStepDataEmpty()) return false;

            var isTutorialEnabled = !_disableTutorial;

            var tutorialChapterIndex = GetCurrentTutorialStep().chapterIndex;
            var tutorialLevelIndex = GetCurrentTutorialStep().levelIndex;

            var isLevelMatchedWithTutorial = (chapterIndex == tutorialChapterIndex) &&
                                             (levelIndex == tutorialLevelIndex) && isTutorialEnabled;

            return isLevelMatchedWithTutorial;
        }

        private bool IsSelectedLevelTutorialCompleted()
        {
            if (IsTutorialStepDataEmpty()) return false;

            var selectedLevelIndex = _levelManager.LastSelectedLevelIndex;

            var isSelectedLevelTutorialCompleted = _completedTutorialLevelData.indexes.Contains(selectedLevelIndex);

            return isSelectedLevelTutorialCompleted;
        }

        private bool IsUnlockedLevelTutorialCompleted()
        {
            var unlockedChapterIndex = _levelManager.LastUnlockedLevelIndex;

            var isUnlockedLevelTutorialCompleted = _completedTutorialLevelData.indexes.Contains(unlockedChapterIndex);

            return isUnlockedLevelTutorialCompleted;
        }

        public void ResetGamePlayTutorial()
        {
            if (IsTutorialStepDataEmpty()) return;

            if (_currentTutorialSubStep == TutorialSubStep.Completed) return;

            _tutorialSubStepIndex = GetCurrentTutorialStep().menuTutorialFinishIndex;
            _tutorialStepManagerSo.tutorialSubStepIndex = _tutorialSubStepIndex;

            _currentTutorialSubStep = (TutorialSubStep)_tutorialSubStepIndex;
            _tutorialStepManagerSo.currentTutorialSubStep = _currentTutorialSubStep;
        }

        public void ResetTutorial()
        {
            if (IsTutorialStepDataEmpty()) return;

            if (_currentTutorialSubStep == TutorialSubStep.Completed) return;

            _tutorialSubStepIndex = 0;
            _tutorialStepManagerSo.tutorialSubStepIndex = _tutorialSubStepIndex;

            _currentTutorialSubStep = (TutorialSubStep)_tutorialSubStepIndex;
            _tutorialStepManagerSo.currentTutorialSubStep = _currentTutorialSubStep;
        }

        private TutorialSubStepData GetCurrentTutorialStep()
        {
            return _tutorialStepData[TutorialStepIndex];
        }

        private string CompletedTutorialLevelName => _levelManager.LastSelectedChapterIndex + "_" + COMPLETED_TURORIAL_LEVEL_KEY;

        private void SetCompletedTutorialLevelIndexes(CompletedTutorialLevelData array)
        {
            _jsonDataManager.Save(CompletedTutorialLevelName, array);
        }

        private CompletedTutorialLevelData GetCompletedTutorialLevelIndexes()
        {
            return _jsonDataManager.Load(CompletedTutorialLevelName, new CompletedTutorialLevelData(), false);
        }

        private bool IsTutorialStepDataEmpty()
        {
            if (_tutorialStepData.Length < 1) return true;

            if (GetCurrentTutorialStep().tutorialSubSteps.Length < 1) return true;

            return false;
        }
    }

    [Serializable]
    public struct TutorialSubStepData
    {
        public int chapterIndex;
        public int levelIndex;
        public int menuTutorialFinishIndex;
        public TutorialSubStep[] tutorialSubSteps;
    }

    [Serializable]
    public class CompletedTutorialLevelData
    {
        public List<int> indexes = new();
    }

    public enum TutorialStep
    {
        Step1 = 0
    }

    public enum TutorialSubStep
    {
        ClickToPlay = 0,
        Completed = 3
    }
}