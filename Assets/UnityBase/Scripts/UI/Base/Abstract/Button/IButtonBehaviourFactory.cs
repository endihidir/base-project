namespace UnityBase.UI.ButtonCore
{
    public interface IButtonBehaviourFactory
    {
        public IButtonBehaviour CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction where TAnim : IUIAnimation;
    }
}