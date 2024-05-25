using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class PlayButton : ButtonBehaviour
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;
    
    protected override void OnClick()
    {
        _sceneManagementService.LoadSceneAsync(SceneType.Gameplay, true, 5f);
    }
}