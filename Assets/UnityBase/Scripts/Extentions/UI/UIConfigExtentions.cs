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
        
        public static IViewBehaviour<TAnim> SetAnimationConfigs<TAnim>(this IViewBehaviour<TAnim> behaviour, params object[] animConfigs) where TAnim : IViewAnimation
        {
            behaviour.ConfigureAnimation(animConfigs);
            return behaviour;
        }
        
        public static IViewBehaviour<TAnim, TModel, TData> SetAnimationConfigs<TAnim, TModel, TData>(this IViewBehaviour<TAnim, TModel, TData> behaviour, params object[] animConfigs)
            where TAnim : IViewAnimation where TModel : IViewModel<TData> where TData : struct
        {
            behaviour.ConfigureAnimation(animConfigs);
            return behaviour;
        }
        
        public static IViewBehaviour<TAnim, TModel, TData> SetModelConfigs<TAnim, TModel, TData>(this IViewBehaviour<TAnim, TModel, TData> behaviour, params object[] modelConfigs)
            where TAnim : IViewAnimation where TModel : IViewModel<TData> where TData : struct
        {
            behaviour.ConfigureModel(modelConfigs);
            return behaviour;
        }
    }
}