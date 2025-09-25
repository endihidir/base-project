using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using VContainer;

namespace UnityBase.Runtime.Factories
{
    public interface IActionFactory
    {
        TAct Resolve<TAct>() where TAct : class, IAction;
        bool TryGet<TAct>(out TAct action) where TAct : class, IAction;
    }
    
    public class ActionFactory : IActionFactory
    {
        private readonly Dictionary<Type, IAction> _actions = new();
        
        private readonly IObjectResolver _resolver;

        public ActionFactory(IObjectResolver resolver) => _resolver = resolver;

        public TAct Resolve<TAct>() where TAct : class, IAction
        {
            var t = typeof(TAct);

            if (!_actions.TryGetValue(t, out var a))
            {
                a = _resolver.CreateInstance<TAct>();
                _actions[t] = a;
            }

            return a as TAct;
        }

        public bool TryGet<TAct>(out TAct action) where TAct : class, IAction
        {
            if (_actions.TryGetValue(typeof(TAct), out var a))
            {
                action = a as TAct;
                return action != null;
            }

            action = null;
            return false;
        }
    }
}