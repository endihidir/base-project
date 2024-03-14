using Sirenix.OdinInspector;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class MainMenuSettingsCloseButton : ButtonBehaviour
{
    [Inject] 
    protected readonly IPopUpDataService _popUpDataService;
    
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
    }
}