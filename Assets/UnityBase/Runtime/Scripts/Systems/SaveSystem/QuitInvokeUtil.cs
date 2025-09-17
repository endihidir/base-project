using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityBase.SaveSystem
{
    public static class QuitInvokeUtil
    {
        private static bool _cached;
        private static readonly List<MethodInfo> StaticMethods = new();
        private static readonly Dictionary<Type, List<MethodInfo>> InstanceMethodsByType = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            CacheIfNeeded();
        }
        
        public static void InvokeAll()
        {
            CacheIfNeeded();
            
            foreach (var m in StaticMethods)
                SafeInvoke(m, null);
            
            if (InstanceMethodsByType.Count > 0)
            {
                var all = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
                if (all != null && all.Length > 0)
                {
                    foreach (var obj in all)
                    {
                        var t = obj.GetType();
                        foreach (var kv in InstanceMethodsByType)
                        {
                            var declType = kv.Key;
                            if (!declType.IsAssignableFrom(t)) continue;

                            var list = kv.Value;
                            foreach (var m in list)
                                SafeInvoke(m, obj);
                        }
                    }
                }
            }
            
            try { PlayerPrefs.Save(); } catch (Exception e) { Debug.LogException(e); }
        }

        private static void CacheIfNeeded()
        {
            if (_cached) return;
            _cached = true;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    var flags = BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Static | BindingFlags.Instance;

                    MethodInfo[] methods;
                    try { methods = t.GetMethods(flags); }
                    catch { continue; }

                    foreach (var m in methods)
                    {
                        if (m.GetCustomAttribute<InvokeOnQuitAttribute>() == null)
                            continue;

                        if (!IsSupportedSignature(m))
                            continue;

                        if (m.IsStatic)
                        {
                            StaticMethods.Add(m);
                        }
                        else
                        {
                            if (!InstanceMethodsByType.TryGetValue(t, out var list))
                                InstanceMethodsByType[t] = list = new List<MethodInfo>();
                            list.Add(m);
                        }
                    }
                }
            }
        }

        private static bool IsSupportedSignature(MethodInfo m) => m.ReturnType == typeof(void) && m.GetParameters().Length == 0;

        private static void SafeInvoke(MethodInfo m, object target)
        {
            try { m.Invoke(target, null); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}