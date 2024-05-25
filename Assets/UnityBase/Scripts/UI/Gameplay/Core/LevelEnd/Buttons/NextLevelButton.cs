using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class NextLevelButton : ButtonBehaviour
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;

    protected override async void OnClick()
    {
        await _sceneManagementService.LoadSceneAsync(SceneType.Gameplay, false, 20f);
    }
}