using UnityBase.Manager;
using UnityBase.Manager.Data;
using UnityBase.UI.Menu;

public class FailMenuGroup : MenuGroup
{
    protected override void OnStartGameStateTransition(GameStateData gameStateData)
    {

    }

    protected override void OnCompleteGameStateTransition(GameStateData gameStateData)
    {
        var openCondition = gameStateData is { StartState: GameState.GamePlayState or GameState.GameTutorialState, EndState: GameState.GameFailState };

        var closeCondition = gameStateData is { StartState: GameState.GameFailState, EndState: GameState.GameLoadingState };

        if (openCondition)
        {
            OpenMenuGroup();
        }
        else if (closeCondition)
        {
            CloseMenuGroup();
        }
    }
}