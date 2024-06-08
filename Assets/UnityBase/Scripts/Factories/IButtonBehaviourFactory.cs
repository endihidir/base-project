
namespace UnityBase.UI.ButtonCore
{
    public interface IButtonBehaviourFactory
    {
        public IButtonBehaviour<TAct, TAnim> CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction 
                                                                                                    where TAnim : IButtonAnimation;
    }
}