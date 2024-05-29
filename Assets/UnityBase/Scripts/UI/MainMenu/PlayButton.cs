using UnityBase.UI.ButtonCore;
using UnityEngine.EventSystems;

public class PlayButton : ButtonUI
{
    protected override void Initialize(IButtonBehaviourFactory buttonBehaviourFactory)
    {
        _buttonBehaviour = buttonBehaviourFactory.CreateButtonBehaviour<SceneLoadAction, UpDownBounceUIAnimation>(this)
                                                 .SetActionConfigs(SceneType.Gameplay, true, 10f);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        _buttonBehaviour.OnClick();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        _buttonBehaviour.OnPointerDown();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        _buttonBehaviour.OnPointerUp();
    }
}