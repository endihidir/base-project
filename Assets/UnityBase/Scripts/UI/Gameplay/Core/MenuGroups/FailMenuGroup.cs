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
        var openCondition = (gameStateData.StartState is GameState.GamePlayState or GameState.GameTutorialState) &&
                            gameStateData.EndState == GameState.GameFailState;

        var closeCondition = gameStateData.StartState == GameState.GameFailState &&
                             gameStateData.EndState == GameState.GameLoadingState;

        if (openCondition)
        {
            OpenAllDependencies();
        }
        else if (closeCondition)
        {
            CloseAllDependencies();
        }
    }
}