using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class NextLevelButton : ButtonUI
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;

    protected override void Initialize(IUIBehaviourFactory uiBehaviourFactory)
    {
        
    }
}