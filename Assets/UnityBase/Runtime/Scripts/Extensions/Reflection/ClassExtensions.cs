using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;

namespace UnityBase.Extensions
{
    public static class ClassExtensions
    {
        public static T CreateInstance<T>(IObjectResolver container, params object[] args) where T : class
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
        
        public static T CreateInstance<T>( params object[] args) where T : class
        {
            var constructor = typeof(T).GetConstructors()[0];
            
            var parameters = constructor.GetParameters();
            
            var finalArgs = new List<object>();

            foreach (var parameter in parameters)
            {
                var matchingArg = args.FirstOrDefault(arg => parameter.ParameterType.IsInstanceOfType(arg));
                
                finalArgs.Add(matchingArg);
            }

            return (T)Activator.CreateInstance(typeof(T), finalArgs.ToArray());
        }
        
        public static void ConfigureMethod<T>(this T target, string methodName, params object[] parameters) where T : class
        {
            var methods = target.GetType().GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == parameters.Length)
                .ToArray();

            MethodInfo configureMethod = null;
            
            foreach (var method in methods)
            {
                var methodParams = method.GetParameters();
                if (ValidateParameters(methodParams, parameters, out var error))
                {
                    configureMethod = method;
                    break;
                }
                
                if(!string.IsNullOrEmpty(error)) 
                    Debug.LogError(error);
            }

            if (configureMethod != null)
            {
                configureMethod.Invoke(target, parameters);
            }
            else
            {
                Debug.LogError($"Method {methodName} with matching parameters not found on {target.GetType().Name}.");
            }
        }

        private static bool ValidateParameters(ParameterInfo[] methodParams, object[] parameters, out string error)
        {
            if (methodParams.Length != parameters.Length)
            {
                error = $"Expected {methodParams.Length} parameters, but got {parameters.Length}.";
                return false;
            }

            for (int i = 0; i < methodParams.Length; i++)
            {
                if (parameters[i] == null || methodParams[i].ParameterType.IsInstanceOfType(parameters[i])) continue;

                error = $"Parameter {i + 1} expected type {methodParams[i].ParameterType.Name}, but got {parameters[i].GetType().Name}.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}