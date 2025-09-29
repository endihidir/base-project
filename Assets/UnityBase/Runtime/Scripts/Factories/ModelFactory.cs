using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using UnityBase.SaveSystem;

namespace UnityBase.Runtime.Factories
{
    public interface IModelFactory
    {
        TModel Resolve<TModel>() where TModel : class, IModel;
    }
    
    public class ModelFactory : IModelFactory
    {
        private readonly Dictionary<Type, IModel> _models = new();
        
        private readonly IAmbientResolverProvider _ambientResolverProvider;
        
        private static readonly HashSet<ISaveData> SaveData = new();

        public ModelFactory(IAmbientResolverProvider ambientResolverProvider) => _ambientResolverProvider = ambientResolverProvider;

        public TModel Resolve<TModel>() where TModel : class, IModel
        {
            var t = typeof(TModel);
            
            if (!_models.TryGetValue(t, out var m))
            {
                var resolver = _ambientResolverProvider.CurrentObjectResolver;
                
                m = resolver.CreateInstance<TModel>();
                
                _models[t] = m;
                
                if (m is ISaveData sd)
                {
                    SaveData.Add(sd);
                }
            }
            
            return m as TModel;
        }

        [InvokeOnQuit]
        public static void SaveAll()
        {
            foreach (var sd in SaveData) sd?.Save();
        }
    }
}