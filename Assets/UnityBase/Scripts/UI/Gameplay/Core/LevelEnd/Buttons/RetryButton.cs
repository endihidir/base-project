using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine.EventSystems;
using VContainer;

public class RetryButton : ButtonUI
{
    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;

    protected override void Initialize(IButtonBehaviourFactory buttonBehaviourFactory)
    {
        
    }

    public override async void OnPointerClick(PointerEventData eventData)
    {
        await _sceneManagementService.LoadSceneAsync(SceneType.Gameplay, false, 2f);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        
    }
}