using UnityBase.Manager;
using UnityBase.Manager.Data;
using UnityBase.UI.Menu;

public class SuccessMenuGroup : MenuGroup
{
    protected override void OnStartGameStateTransition(GameStateData gameStateData)
    {
        var openCondition = gameStateData is { StartState: GameState.GamePlayState or GameState.GameTutorialState, EndState: GameState.GameSuccessState };
        
        var closeCondition = gameStateData is { StartState: GameState.GameSuccessState, EndState: GameState.GameLoadingState };

        if (openCondition)
        {
            OpenMenuGroup();
        }
        else if (closeCondition)
        {
            CloseMenuGroup();
        }
    }

    protected override void OnCompleteGameStateTransition(GameStateData gameStateData)
    {
        
    }
}