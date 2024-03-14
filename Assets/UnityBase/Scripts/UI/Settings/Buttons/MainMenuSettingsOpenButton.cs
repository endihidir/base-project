using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class MainMenuSettingsOpenButton : ButtonBehaviour
{
    [Inject] 
    private readonly IPopUpDataService _popUpDataService;

    protected override void OnClick()
    {
        _popUpDataService.GetPopUp<MainMenuSettingsPopUp>();
    }
}