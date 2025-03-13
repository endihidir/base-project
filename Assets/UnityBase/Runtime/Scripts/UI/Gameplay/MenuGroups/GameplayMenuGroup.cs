using UnityBase.Manager;
using UnityBase.Manager.Data;
using UnityBase.UI.Menu;

public class GameplayMenuGroup : MenuGroup
{
    protected override void OnStartGameStateTransition(GameStateData gameStateData)
    {
        var openCondition = gameStateData is { StartState: GameState.GameLoadingState, EndState: GameState.GamePlayState or GameState.GameTutorialState };

        var closeCondition = gameStateData is { StartState: GameState.GamePlayState or GameState.GameTutorialState, EndState: GameState.GameFailState or GameState.GameSuccessState};

        if (openCondition)
        {
            OpenMenuGroup();
        }

        else if (closeCondition) CloseMenuGroup();
    }

    protected override void OnCompleteGameStateTransition(GameStateData gameStateData)
    {
       
    }
}