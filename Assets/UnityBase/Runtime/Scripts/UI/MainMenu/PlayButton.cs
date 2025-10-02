using DG.Tweening;
using UnityBase.UI.ButtonCore;

public class PlayButton : ButtonBase
{
    protected override void Initialize()
    {
        base.Initialize();
        
        _buttonAction = OwnerContext.ResolveAction<SceneLoadAction>()
                               .Configure("Gameplay", true);
        
        _buttonAnimation = OwnerContext.ResolveAnimation<ButtonClickAnim>()
                                  .SetButtonTransform(Button.transform)
                                  .Configure(1.05f, 0.1f, Ease.InOutQuad);
    }
}