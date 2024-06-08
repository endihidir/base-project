using UnityBase.Extensions;
using UnityBase.Manager;
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
        
        public IButtonBehaviour<TAct, TAnim> CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction where TAnim : IButtonAnimation
        {
            var buttonAction = ReflectionExtensions.CreateInstance<TAct>(_container, buttonUI);
            
            var buttonAnimation = ReflectionExtensions.CreateInstance<TAnim>(_container, buttonUI);
            
            return new ButtonBehaviour<TAct, TAnim>(buttonAction, buttonAnimation);
        }
    }
}