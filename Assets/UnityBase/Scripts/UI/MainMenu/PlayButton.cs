using DG.Tweening;
using UnityBase.Extensions;
using UnityBase.UI.ButtonCore;

public class PlayButton : ButtonUI
{
    private IButtonBehaviourFactory _buttonBehaviourFactory;
    
    protected override void Initialize(IButtonBehaviourFactory buttonBehaviourFactory)
    {
        _buttonBehaviourFactory = buttonBehaviourFactory;
        
        _buttonBehaviour = _buttonBehaviourFactory.CreateButtonBehaviour<SceneLoadAction, ButtonClickAnim>(this)
                                                 .SetActionConfigs(SceneType.Gameplay, true, 10f)
                                                 .SetAnimationConfigs(1.05f, 1f, 0.1f, Ease.InOutQuad);
    }
}