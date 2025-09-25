using System;

namespace UnityBase.StateMachineCore
{
    public static class TreeStateExtensions
    {
        public static IStateNode AddOnInit(this IStateNode s, Action a) { s.OnInitState += a; return s; }
        public static IStateNode AddOnBeforeEnter(this IStateNode s, Action a) { s.OnBeforeEnterState += a; return s; }
        public static IStateNode AddOnEnter(this IStateNode s, Action a) { s.OnEnterState += a; return s; }
        public static IStateNode AddOnUpdate(this IStateNode s, Action<float> a) { s.OnUpdateState += a; return s; }
        public static IStateNode AddOnFixedUpdate(this IStateNode s, Action<float> a) { s.OnFixedUpdateState += a; return s; }
        public static IStateNode AddOnLateUpdate(this IStateNode s, Action<float> a) { s.OnLateUpdateState += a; return s; }
        public static IStateNode AddOnExit(this IStateNode s, Action a) { s.OnExitState += a; return s; }

        public static IStateNode RemoveOnInit(this IStateNode s, Action a) { s.OnInitState -= a; return s; }
        public static IStateNode RemoveOnBeforeEnter(this IStateNode s, Action a) { s.OnBeforeEnterState -= a; return s; }
        public static IStateNode RemoveOnEnter(this IStateNode s, Action a) { s.OnEnterState -= a; return s; }
        public static IStateNode RemoveOnUpdate(this IStateNode s, Action<float> a) { s.OnUpdateState -= a; return s; }
        public static IStateNode RemoveOnFixedUpdate(this IStateNode s, Action<float> a) { s.OnFixedUpdateState -= a; return s; }
        public static IStateNode RemoveOnLateUpdate(this IStateNode s, Action<float> a) { s.OnLateUpdateState -= a; return s; }
        public static IStateNode RemoveOnExit(this IStateNode s, Action a) { s.OnExitState -= a; return s; }
    }
}