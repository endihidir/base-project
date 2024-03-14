using Sirenix.OdinInspector;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class SettingsMainMenuButton : LevelEndMainMenuButton
{
    [Inject]
    protected readonly ITutorialDataService _tutorialDataService;

    [Inject] 
    protected readonly IPopUpDataService _popUpDataService;

    [Inject] 
    protected readonly ITutorialStepDataService _tutorialStepDataService;
    
    [ReadOnly] [SerializeField] private PopUp _popUp;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        _popUp = GetComponentInParent<PopUp>(true);
    }
#endif
    
    protected override void OnClick()
    {
        base.OnClick();

        _popUpDataService.HidePopUp(_popUp);
        
        _tutorialStepDataService.ResetTutorial();
    }
}