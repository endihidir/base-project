using System;
using System.Collections.Generic;
using UnityBase.Extensions;
using VContainer;

namespace UnityBase.Runtime.Factories
{
    public interface IOwnerContextGroup
    {
        TView ResolveView<TView>() where TView : class, IView;
        TAnim ResolveAnimation<TAnim>() where TAnim : class, IAnimation;
        TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter;
        bool TryGetPresenter<TPresenter>(out TPresenter value) where TPresenter : class, IPresenter;
        void Dispose();
    }

    public class OwnerContextGroup : IOwnerContextGroup
    {
        private readonly IDictionary<Type, IView> _views = new Dictionary<Type, IView>();
        private readonly IDictionary<Type, IAnimation> _animations = new Dictionary<Type, IAnimation>();
        private readonly IDictionary<Type, IPresenter> _presenters = new Dictionary<Type, IPresenter>();

        private readonly IObjectResolver _resolver;

        public OwnerContextGroup(IObjectResolver resolver) => _resolver = resolver;
        
        public TView ResolveView<TView>() where TView : class, IView => GetOrCreate<IView, TView>(_views);
        public TAnim ResolveAnimation<TAnim>() where TAnim : class, IAnimation => GetOrCreate<IAnimation, TAnim>(_animations);
        public TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter => GetOrCreate<IPresenter, TPresenter>(_presenters);
        

        private TImpl GetOrCreate<TIFace, TImpl>(IDictionary<Type, TIFace> map) where TIFace : class where TImpl : class, TIFace
        {
            var key = typeof(TImpl);
            
            if (!map.TryGetValue(key, out var obj))
            {
                obj = _resolver.CreateInstance<TImpl>(_resolver);
                map[key] = obj;
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
                    map.TryAdd(iface, obj);
            }
        }
        
        public bool TryGetPresenter<TPresenter>(out TPresenter value) where TPresenter : class, IPresenter => TryGet(_presenters, out value);

        private static bool TryGet<TIFace, TImpl>(IDictionary<Type, TIFace> map, out TImpl typed) where TIFace : class where TImpl : class, TIFace
        {
            if (map.TryGetValue(typeof(TImpl), out var obj))
            {
                typed = obj as TImpl;
                return typed != null;
            }

            foreach (var v in map.Values)
            {
                if (v is TImpl casted) { typed = casted; return true; }
            }

            typed = null;
            return false;
        }
        
        public void Dispose()
        {
            DisposeAll(_presenters.Values);
            DisposeAll(_animations.Values);
            DisposeAll(_views.Values);

            _presenters.Clear();
            _animations.Clear();
            _views.Clear();
        }

        private static void DisposeAll<T>(IEnumerable<T> items)
        {
            foreach (var it in items)
                if (it is IDisposable d) d.Dispose();
        }
    }
}
