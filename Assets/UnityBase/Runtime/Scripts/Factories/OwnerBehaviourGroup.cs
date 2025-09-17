using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using UnityBase.SaveSystem;
using VContainer;

namespace UnityBase.Runtime.Behaviours
{
    public interface IOwnerBehaviourGroup
    {
        TView  CreateView<TView>() where TView  : class, IView;
        TModel CreateModel<TModel>() where TModel : class, IModel;
        TAnim  CreateAnimation<TAnim>() where TAnim : class, IAnimation;
        TAct   CreateAction<TAct>() where TAct   : class, IAction;
        TCtrl  CreateController<TCtrl>() where TCtrl : class, IController;

        bool TryGetView<TView>(out TView value) where TView : class, IView;
        bool TryGetModel<TModel>(out TModel value) where TModel : class, IModel;
        bool TryGetAnimation<TAnim>(out TAnim value) where TAnim : class, IAnimation;
        bool TryGetAction<TAct>(out TAct value) where TAct : class, IAction;
        bool TryGetController<TCtrl>(out TCtrl value) where TCtrl : class, IController;
        void Dispose();
    }

    public class OwnerBehaviourGroup : IOwnerBehaviourGroup
    {
        private readonly IDictionary<Type, IView> _views = new Dictionary<Type, IView>();
        private readonly IDictionary<Type, IModel> _models = new Dictionary<Type, IModel>();
        private readonly IDictionary<Type, IAnimation> _animations = new Dictionary<Type, IAnimation>();
        private readonly IDictionary<Type, IAction> _actions = new Dictionary<Type, IAction>();
        private readonly IDictionary<Type, IController> _controllers = new Dictionary<Type, IController>();
        private static readonly HashSet<ISaveData> SaveData = new();

        private readonly IObjectResolver _resolver;

        public OwnerBehaviourGroup(IObjectResolver resolver) => _resolver = resolver;

        public TView  CreateView<TView>() where TView : class, IView => GetOrCreate<IView,TView>(_views);
        public TModel CreateModel<TModel>() where TModel : class, IModel => GetOrCreate<IModel,TModel>(_models);
        public TAnim  CreateAnimation<TAnim>() where TAnim : class, IAnimation => GetOrCreate<IAnimation,TAnim>(_animations);
        public TAct   CreateAction<TAct>() where TAct : class, IAction => GetOrCreate<IAction,TAct>(_actions);
        public TCtrl  CreateController<TCtrl>() where TCtrl : class, IController => GetOrCreate<IController,TCtrl>(_controllers);

        private TImpl GetOrCreate<TIFace, TImpl>(IDictionary<Type, TIFace> map) where TIFace : class where TImpl  : class, TIFace
        {
            var key = typeof(TImpl);

            if (!map.TryGetValue(key, out var obj))
            {
                obj = ClassExtensions.CreateInstance<TImpl>(_resolver);
                map[key] = obj;
                
                if (obj is ISaveData saveData)
                    SaveData.Add(saveData);
                
                IndexAssignableInterfaces(map, obj);
            }

            return obj as TImpl;
        }

        private static void IndexAssignableInterfaces<TIFace>(IDictionary<Type, TIFace> map, TIFace obj) where TIFace : class
        {
            var implType = obj.GetType();
            
            var ifaces = implType.GetInterfaces();
            
            for (int i = 0; i < ifaces.Length; i++)
            {
                var iface = ifaces[i];
                
                if (typeof(TIFace).IsAssignableFrom(iface))
                {
                    map.TryAdd(iface, obj);
                }
            }
        }

        public bool TryGetController<TCtrl>(out TCtrl value) where TCtrl : class, IController => TryGet(_controllers, out value);

        public bool TryGetView<TView>(out TView value) where TView : class, IView => TryGet(_views, out value);
        public bool TryGetModel<TModel>(out TModel value) where TModel : class, IModel => TryGet(_models, out value);
        public bool TryGetAnimation<TAnim>(out TAnim value) where TAnim : class, IAnimation => TryGet(_animations, out value);
        public bool TryGetAction<TAct>(out TAct value) where TAct : class, IAction => TryGet(_actions, out value);

        private static bool TryGet<TIFace, TImpl>(IDictionary<Type, TIFace> map, out TImpl typed) where TIFace : class where TImpl  : class, TIFace
        {
            if (map.TryGetValue(typeof(TImpl), out var obj))
            {
                typed = obj as TImpl;
                return typed != null;
            }
            
            foreach (var v in map.Values)
            {
                if (v is TImpl casted)
                {
                    typed = casted;
                    return true;
                }
            }

            typed = null;
            return false;
        }
        
        public void Dispose()
        {
            RemoveSaveData();
        }

        private void RemoveSaveData()
        {
            foreach (var v in _models.Values)
                if (v is ISaveData sd) SaveData.Remove(sd);
            
            foreach (var v in _views.Values)
                if (v is ISaveData sd) SaveData.Remove(sd);
            
            foreach (var v in _animations.Values)
                if (v is ISaveData sd) SaveData.Remove(sd);
            
            foreach (var v in _actions.Values)
                if (v is ISaveData sd) SaveData.Remove(sd);
            
            foreach (var v in _controllers.Values)
                if (v is ISaveData sd) SaveData.Remove(sd);
        }

        [InvokeOnQuit]
        public static void SaveAll()
        {
            foreach (var saveData in SaveData)
            {
                saveData?.Save();
            }
        }
    }
}
