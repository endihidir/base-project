using UnityBase.Manager;
using UnityBase.Manager.Data;
using UnityBase.UI.Menu;

public class SuccessMenuGroup : MenuGroup
{
    protected override void OnStartGameStateTransition(GameStateData gameStateData)
    {

    }

    protected override void OnCompleteGameStateTransition(GameStateData gameStateData)
    {
        var openCondition = (gameStateData.StartState is GameState.GamePlayState or GameState.GameTutorialState) &&
                            gameStateData.EndState == GameState.GameSuccessState;
        var closeCondition = gameStateData.StartState == GameState.GameSuccessState &&
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