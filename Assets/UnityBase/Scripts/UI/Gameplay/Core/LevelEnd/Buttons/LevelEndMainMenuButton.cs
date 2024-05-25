using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class LevelEndMainMenuButton : ButtonBehaviour
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;
    
    protected override async void OnClick()
    {
        await _sceneManagementService.LoadSceneAsync(SceneType.MainMenu, true, 5f);
    }
}