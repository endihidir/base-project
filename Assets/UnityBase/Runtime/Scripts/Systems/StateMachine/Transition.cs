using System;

namespace UnityBase.StateMachineCore
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        int Priority { get; }
        bool OneShot { get; }
        bool RequestTransition();
    }

    public sealed class Transition : ITransition
    {
        public IState From { get; }
        public IState To { get; }
        public int Priority { get; }
        public bool OneShot { get; }

        private readonly Func<bool> _condition;

        public Transition(IState from, IState to, Func<bool> condition, int priority = 0, bool oneShot = false)
        {
            From = from;
            To = to;
            _condition =  condition ?? (() => false);
            Priority = priority;
            OneShot = oneShot;

            if (from is IStateNode f && to is IStateNode t && f.GetRootState() != t.GetRootState())
                DebugLogger.LogError($"Invalid transition: '{(f.StateID ?? "null")}' and '{(t.StateID ?? "null")}' are in different roots.");
        }

        public bool RequestTransition() => _condition();
    }
}