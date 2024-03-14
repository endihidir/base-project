using UnityBase.Manager;

namespace UnityBase.Service
{
    public interface ITutorialStepDataService
    {
        public bool IsUnlockedLevelTutorialEnabled { get; }
        public bool IsSelectedLevelTutorialEnabled { get; }
        public TutorialSubStep GetCurrentTutorialSubStep(int index);
        public void ResetGamePlayTutorial();
        public void ResetTutorial();
    }
}