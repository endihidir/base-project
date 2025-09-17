using System;

namespace UnityBase.StateMachineCore
{
    public static class TreeStateEventExtensions
    {
        public static ITreeState AddOnInit(this ITreeState s, Action a)                    { s.OnInitState        += a; return s; }
        public static ITreeState AddOnBeforeEnter(this ITreeState s, Action a)            { s.OnBeforeEnterState += a; return s; }
        public static ITreeState AddOnEnter(this ITreeState s, Action a)                  { s.OnEnterState       += a; return s; }
        public static ITreeState AddOnUpdate(this ITreeState s, Action<float> a)          { s.OnUpdateState      += a; return s; }
        public static ITreeState AddOnFixedUpdate(this ITreeState s, Action<float> a)     { s.OnFixedUpdateState += a; return s; }
        public static ITreeState AddOnLateUpdate(this ITreeState s, Action<float> a)      { s.OnLateUpdateState  += a; return s; }
        public static ITreeState AddOnExit(this ITreeState s, Action a)                   { s.OnExitState        += a; return s; }

        public static ITreeState RemoveOnInit(this ITreeState s, Action a)                { s.OnInitState        -= a; return s; }
        public static ITreeState RemoveOnBeforeEnter(this ITreeState s, Action a)         { s.OnBeforeEnterState -= a; return s; }
        public static ITreeState RemoveOnEnter(this ITreeState s, Action a)               { s.OnEnterState       -= a; return s; }
        public static ITreeState RemoveOnUpdate(this ITreeState s, Action<float> a)       { s.OnUpdateState      -= a; return s; }
        public static ITreeState RemoveOnFixedUpdate(this ITreeState s, Action<float> a)  { s.OnFixedUpdateState -= a; return s; }
        public static ITreeState RemoveOnLateUpdate(this ITreeState s, Action<float> a)   { s.OnLateUpdateState  -= a; return s; }
        public static ITreeState RemoveOnExit(this ITreeState s, Action a)                { s.OnExitState        -= a; return s; }
    }
}