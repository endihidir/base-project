using DG.Tweening;
using UnityBase.UI.ButtonCore;
using UnityBase.UI.ViewCore;

public class PlayButton : ButtonBase
{
    protected override void Initialize(IViewBehaviourFactory viewBehaviourFactory)
    {
        _buttonAction = viewBehaviourFactory.CreateAction<SceneLoadAction>(this)
                                              .Configure(SceneType.Gameplay, true);
        
        _buttonAnimation = viewBehaviourFactory.CreateAnimation<ButtonClickAnim>(this)
                                               .SetButtonTransform(Button.transform)
                                               .Configure(1.05f, 0.1f, Ease.InOutQuad);
    }
}