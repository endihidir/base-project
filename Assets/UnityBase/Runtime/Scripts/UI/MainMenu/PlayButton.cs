using DG.Tweening;
using UnityBase.UI.ButtonCore;
using UnityBase.Runtime.Factories;

public class PlayButton : ButtonBase
{
    protected override void Initialize(IOwnerContextFactory ownerContextFactory)
    {
        var context = ownerContextFactory.GetContext(this);
        
        _buttonAction = context.ResolveAction<SceneLoadAction>()
                               .Configure(SceneType.Gameplay, true);
        
        _buttonAnimation = context.ResolveAnimation<ButtonClickAnim>()
                                  .SetButtonTransform(Button.transform)
                                  .Configure(1.05f, 0.1f, Ease.InOutQuad);
    }
}