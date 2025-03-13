using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UnityBase.EventBus.Utils
{
    public static class EventBusUtil
    {
        public static IReadOnlyList<Type> EventTypes { get; set; }
        public static IReadOnlyList<Type> EventBusTypes { get; set; }
    
    #if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; set; }
    
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
    
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            PlayModeState = state;
            
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllBuses();
            }
        }
    #endif
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            
            EventBusTypes = InitializeAllBusses();
        }
    
        private static List<Type> InitializeAllBusses()
        {
            var typedef = typeof(EventBus<>);

            return EventTypes.Select(eventType => typedef.MakeGenericType(eventType)).ToList();
        }
    
        private static void ClearAllBuses()
        {
            //Debug.Log("Clearing all buses...");

            foreach (var busType in EventBusTypes)
            {
                var clearMethod = busType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
                
                clearMethod?.Invoke(null, null);
            }
        }
    }
}