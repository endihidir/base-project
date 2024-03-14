using UnityBase.Manager;
using UnityEngine;

namespace UnityBase.TutorialCore.Handler
{
    public class GameplayTutorialHandler : MonoBehaviour
    {

#if UNITY_EDITOR
        private void OnValidate()
        {
            name = nameof(GameplayTutorialHandler);
        }
#endif

        private void OnEnable() => TutorialStepManager.OnUpdateTutorialSubStep += OnUpdateTutorialSubStep;

        private void OnDisable() => TutorialStepManager.OnUpdateTutorialSubStep -= OnUpdateTutorialSubStep;

        private void OnUpdateTutorialSubStep(TutorialSubStep tutorialSubStep)
        {
            switch (tutorialSubStep)
            {
                case TutorialSubStep.ClickToPlay:
                    TutorialStepManager.OnCompleteTutorialStep.Invoke();
                    return;
            }
        }
    }
}