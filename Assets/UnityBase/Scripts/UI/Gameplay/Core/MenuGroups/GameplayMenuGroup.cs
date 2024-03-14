using UnityBase.Manager;
using UnityBase.Manager.Data;
using UnityBase.UI.Menu;

public class GameplayMenuGroup : MenuGroup
{
    protected override void OnStartGameStateTransition(GameStateData gameStateData)
    {
        var openCondition = gameStateData.StartState == GameState.GameLoadingState &&
                            gameStateData.EndState is GameState.GamePlayState or GameState.GameTutorialState;
        var closeCondition = gameStateData.StartState is GameState.GamePlayState or GameState.GameTutorialState &&
                             gameStateData.EndState is GameState.GameFailState or GameState.GameSuccessState;

        if (openCondition)
        {
            OpenAllDependencies();
        }

        else if (closeCondition) CloseAllDependencies();
    }

    protected override void OnCompleteGameStateTransition(GameStateData gameStateData)
    {
       
    }
}