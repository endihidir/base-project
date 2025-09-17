using DG.Tweening;
using UnityBase.UI.ButtonCore;
using UnityBase.Runtime.Behaviours;

public class PlayButton : ButtonBase
{
    protected override void Initialize(IOwnerBehaviourFactory ownerBehaviourFactory)
    {
        var context = ownerBehaviourFactory.RegisterAndGetContext(this);
        
        _buttonAction = context.CreateAction<SceneLoadAction>()
                               .Configure(SceneType.Gameplay, true);
        
        _buttonAnimation = context.CreateAnimation<ButtonClickAnim>()
                                  .SetButtonTransform(Button.transform)
                                  .Configure(1.05f, 0.1f, Ease.InOutQuad);
    }
}