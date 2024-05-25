using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class RetryButton : ButtonBehaviour
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;

    protected override async void OnClick()
    {
        await _sceneManagementService.LoadSceneAsync(SceneType.Gameplay, false, 2f);
    }
}