using DG.Tweening;
using UnityBase.Extensions;
using UnityBase.UI.ButtonCore;

public class PlayButton : ButtonUI
{
    private IUIBehaviourFactory _uiBehaviourFactory;
    
    protected override void Initialize(IUIBehaviourFactory uiBehaviourFactory)
    {
        _uiBehaviourFactory = uiBehaviourFactory;
        
        _buttonBehaviour = _uiBehaviourFactory.CreateButtonBehaviour<SceneLoadAction, ButtonClickAnim>(this)
                                                 .SetActionConfigs(SceneType.Gameplay, true, 10f)
                                                 .SetAnimationConfigs(1.05f, 1f, 0.1f, Ease.InOutQuad);
    }
}