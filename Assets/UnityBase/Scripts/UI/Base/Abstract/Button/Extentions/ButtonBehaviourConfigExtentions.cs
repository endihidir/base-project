namespace UnityBase.UI.ButtonCore
{
    public static class ButtonBehaviourConfigExtentions
    {
        public static IButtonBehaviour SetActionConfigs(this IButtonBehaviour behaviour, params object[] actConfigs)
        {
            behaviour.ConfigureAction(actConfigs);
            return behaviour;
        }
        
        public static IButtonBehaviour SetAnimationConfigs(this IButtonBehaviour behaviour, params object[] animConfigs)
        {
            behaviour.ConfigureAnimation(animConfigs);
            return behaviour;
        }
    }
}