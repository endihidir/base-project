using UnityBase.Extensions;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviourFactory : IButtonBehaviourFactory
    {
        private readonly IObjectResolver _container;

        public ButtonBehaviourFactory(IObjectResolver container)
        {
            _container = container;
        }

        public TAct CreateButtonAction<TAct>(IButtonUI buttonUI) where TAct : class, IButtonAction
        {
          
            
            return ClassExtensions.CreateInstance<TAct>(_container, buttonUI);
        }

        public TAnim CreateButtonAnimation<TAnim>(IButtonUI buttonUI) where TAnim : class, IButtonAnimation
        {
            return ClassExtensions.CreateInstance<TAnim>(_container, buttonUI);
        }
    }
}