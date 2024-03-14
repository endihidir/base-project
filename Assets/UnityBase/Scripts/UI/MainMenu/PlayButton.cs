using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using VContainer;

public class PlayButton : ButtonBehaviour
{
    [Inject] 
    private readonly ISceneDataService _sceneDataService;
    
    protected override void OnClick()
    {
        _sceneDataService.LoadSceneAsync(SceneType.Gameplay, true, 5f);
    }
}