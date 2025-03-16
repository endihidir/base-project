
namespace UnityBase.UI.ButtonCore
{
    public interface IButtonBehaviourFactory
    {
        public TAct CreateButtonAction<TAct>(IButtonUI buttonUI) where TAct : class, IButtonAction;
        public TAnim CreateButtonAnimation<TAnim>(IButtonUI buttonUI) where TAnim : class, IButtonAnimation;
    }
}