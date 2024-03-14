using UnityBase.Manager;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/TutorialStepManagerData")]
    public class TutorialStepManagerSO : ScriptableObject
    {
        public bool disableTutorial;

        public TutorialSubStepData[] tutorialStepData;

        public TutorialStep currentTutorialStep;

        public int tutorialSubStepIndex = 0;

        public TutorialSubStep currentTutorialSubStep;

        public void Initialize()
        {

        }
    }
}
