using UnityBase.UI.ButtonCore;

namespace UnityBase.Extensions
{
    public static class UIConfigExtentions
    {
        public static IButtonBehaviour<TAct, TAnim> SetActionConfigs<TAct, TAnim>(this IButtonBehaviour<TAct, TAnim> behaviour, params object[] actConfigs)
            where TAct : IButtonAction where TAnim : IButtonAnimation
        {
            behaviour.ConfigureAction(actConfigs);
            return behaviour;
        }
        
        public static IButtonBehaviour<TAct, TAnim> SetAnimationConfigs<TAct, TAnim>(this IButtonBehaviour<TAct, TAnim> behaviour, params object[] animConfigs)
            where TAct : IButtonAction where TAnim : IButtonAnimation
        {
            behaviour.ConfigureAnimation(animConfigs);
            return behaviour;
        }
    }
}