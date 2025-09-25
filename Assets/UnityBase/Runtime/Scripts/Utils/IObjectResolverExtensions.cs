using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace UnityBase.Extensions
{
    public static class IObjectResolverExtensions
    {
        public static T CreateInstance<T>(this IObjectResolver container, params object[] args) where T : class
        {
            var constructor = typeof(T).GetConstructors()[0];
            
            var parameters = constructor.GetParameters();
            
            var finalArgs = new List<object>();

            foreach (var parameter in parameters)
            {
                var matchingArg = args.FirstOrDefault(arg => parameter.ParameterType.IsInstanceOfType(arg));
                
                finalArgs.Add(matchingArg ?? container.Resolve(parameter.ParameterType));
            }

            return (T)Activator.CreateInstance(typeof(T), finalArgs.ToArray());
        }
    }
}