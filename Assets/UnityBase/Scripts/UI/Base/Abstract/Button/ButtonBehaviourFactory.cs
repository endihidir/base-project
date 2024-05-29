using System;
using System.Collections.Generic;
using VContainer;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviourFactory : IButtonBehaviourFactory
    {
        private readonly IObjectResolver _container;

        public ButtonBehaviourFactory(IObjectResolver container) => _container = container;

        public IButtonBehaviour CreateButtonBehaviour<TAct, TAnim>(IButtonUI buttonUI) where TAct : IButtonAction where TAnim : IUIAnimation
        {
            var buttonAction = CreateInstance<TAct>(buttonUI);
            var buttonAnimation = CreateInstance<TAnim>(buttonUI);
            return new ButtonBehaviour<TAct, TAnim>(buttonAction, buttonAnimation);
        }

        private T CreateInstance<T>(IButtonUI buttonUI)
        {
            var constructor = typeof(T).GetConstructors()[0];
            var parameters = constructor.GetParameters();
            var args = new List<object> { buttonUI };
        
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType != typeof(IButtonUI))
                {
                    args.Add(_container.Resolve(parameter.ParameterType));
                }
            }
        
            return (T)Activator.CreateInstance(typeof(T), args.ToArray());
        }
    }
}