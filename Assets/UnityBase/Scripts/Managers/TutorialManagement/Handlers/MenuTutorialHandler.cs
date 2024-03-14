using UnityBase.Manager;
using UnityBase.Service;
using UnityEngine;
using VContainer;

namespace UnityBase.TutorialCore.Handler
{
    public class MenuTutorialHandler : MonoBehaviour
    {
        [Inject] 
        private readonly ITutorialDataService _tutorialDataService;

        [Inject] 
        private readonly ITutorialStepDataService _tutorialStepDataService;

#if UNITY_EDITOR
        private void OnValidate()
        {
            name = nameof(MenuTutorialHandler);
        }
#endif

        private void Awake()
        {
            
        }

        private void Start()
        {
            if (_tutorialStepDataService.IsUnlockedLevelTutorialEnabled)
            {
                //OnUpdateTutorialStep(_tutorialManager.TutorialStepController.GetCurrentTutorialSubStep(0));
            }
        }

        private void OnEnable()
        {
            TutorialStepManager.OnUpdateTutorialSubStep += OnUpdateTutorialStep;
        }

        private void OnDisable()
        {
            TutorialStepManager.OnUpdateTutorialSubStep -= OnUpdateTutorialStep;
        }

        private void OnUpdateTutorialStep(TutorialSubStep tutorialSubStep)
        {
            switch (tutorialSubStep)
            {
                case TutorialSubStep.ClickToPlay:
                    break;
            }
        }
    }
}