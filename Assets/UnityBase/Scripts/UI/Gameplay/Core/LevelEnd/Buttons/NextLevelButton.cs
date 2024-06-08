using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class NextLevelButton : ButtonUI
{
    [Inject] 
    private readonly ISceneManager _sceneManager;

    protected override void Initialize(IButtonBehaviourFactory buttonBehaviourFactory)
    {
        
    }
}