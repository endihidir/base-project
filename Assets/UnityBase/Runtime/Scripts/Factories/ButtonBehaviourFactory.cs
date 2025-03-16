using UnityBase.Extensions;
using UnityBase.Manager;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviourFactory : IButtonBehaviourFactory
    {
        private IObjectResolverContainer _objectResolverContainer;

        public ButtonBehaviourFactory(IObjectResolverContainer resolverContainer)
        {
            _objectResolverContainer = resolverContainer;
        }
        
        public TAct CreateButtonAction<TAct>(IButtonUI buttonUI) where TAct : class, IButtonAction
        {
            return ClassExtensions.CreateInstance<TAct>(_objectResolverContainer.ObjectResolver, buttonUI);
        }

        public TAnim CreateButtonAnimation<TAnim>(IButtonUI buttonUI) where TAnim : class, IButtonAnimation
        {
            return ClassExtensions.CreateInstance<TAnim>(_objectResolverContainer.ObjectResolver, buttonUI);
        }
    }
}