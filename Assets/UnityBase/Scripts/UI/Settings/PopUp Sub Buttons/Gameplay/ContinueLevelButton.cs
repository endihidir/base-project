using Sirenix.OdinInspector;
using UnityBase.Manager;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class ContinueLevelButton : ButtonBehaviour
{
    [Inject] 
    private readonly IGameplayDataService _gameplayDataService;

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
        _popUpDataService.HidePopUp(_popUp);
        
        ChangeState();
    }

    private void ChangeState()
    {
        var selectedState = _tutorialStepDataService.IsSelectedLevelTutorialEnabled ? GameState.GameTutorialState : GameState.GamePlayState;
            
        _gameplayDataService.ChangeGameState(selectedState, 0f);
    }
}