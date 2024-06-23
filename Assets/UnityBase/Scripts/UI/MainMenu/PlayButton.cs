using DG.Tweening;
using UnityBase.UI.ButtonCore;

public class PlayButton : ButtonUI
{
    protected override void Initialize(IButtonBehaviourFactory buttonBehaviourFactory)
    {
        _buttonAction = buttonBehaviourFactory.CreateButtonAction<SceneLoadAction>(this)
                                              .Configure(SceneType.Gameplay, true);
        
        _buttonAnimation = buttonBehaviourFactory.CreateButtonAnimation<ButtonClickAnim>(this)
                                                 .Configure(1.05f, 0.1f, Ease.InOutQuad);
    }
}