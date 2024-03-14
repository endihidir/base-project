using UnityBase.Manager;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class GameplaySettingsOpenButton : ButtonBehaviour
{
    [Inject] 
    private readonly IGameplayDataService _gameplayDataService;

    [Inject] 
    protected readonly IPopUpDataService _popUpDataService;

    protected override void OnClick()
    {
        _popUpDataService.GetPopUp<GameplaySettingsPopUp>();

        _gameplayDataService.ChangeGameState(GameState.GamePauseState, 0f);
    }
}