using UnityBase.Extensions;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviourFactory : IButtonBehaviourFactory
    {
        private IObjectResolver _resolver;

        public ButtonBehaviourFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void UpdateResolver(IObjectResolver resolver)
        {
            if(_resolver.Equals(resolver)) return;
            
            _resolver = resolver;
        }

        public TAct CreateButtonAction<TAct>(IButtonUI buttonUI) where TAct : class, IButtonAction
        {
            return ClassExtensions.CreateInstance<TAct>(_resolver, buttonUI);
        }

        public TAnim CreateButtonAnimation<TAnim>(IButtonUI buttonUI) where TAnim : class, IButtonAnimation
        {
            return ClassExtensions.CreateInstance<TAnim>(_resolver, buttonUI);
        }
    }
}