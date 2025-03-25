using System;

namespace UnityBase.StateMachineCore
{
    public enum TransitionMode { Persistent, OneShot }
    public interface IGenericTransition<out TState>
    {
        public TState FromID { get; }
        public TState ToID { get; }
        public IState ToState { get; }
        public TransitionMode Mode { get; }
        public bool Check();
    }
    
    public interface IGenericEventTransition<out TState, out TEvent>
    {
        public TState FromID { get; }
        public TState ToID { get; }
        public IState ToState { get; }
        public TEvent TriggerEvent { get; }
    }
    
    public class GenericTransition<TState> : IGenericTransition<TState>
    {
        public TState FromID { get; }
        public TState ToID { get; }
        public TransitionMode Mode { get; }
        
        private readonly ITransition _transition;    
        
        private bool _used;

        public GenericTransition(TState from, TState to, IState fromState, IState toState, Func<bool> condition = null, TransitionMode mode = TransitionMode.Persistent)
        {
            FromID = from;
            
            ToID = to;
            
            _transition = new TransitionBase(fromState, toState, condition);
            
            Mode = mode;
        }

        public bool Check()
        {
            if (_used && Mode == TransitionMode.OneShot)
                return false;

            if (!_transition.CheckTrigger()) return false;
            
            if (Mode == TransitionMode.OneShot)
                _used = true;

            return true;
        }
        
        public IState ToState => _transition.To;
    }

    public class GenericEventTransition<TState, TEvent> : IGenericEventTransition<TState, TEvent>
    {
        public TState FromID { get; }
        public TState ToID { get; }
        public TEvent TriggerEvent { get; }
        public IState ToState { get; }

        public GenericEventTransition(TState from, TState to, TEvent triggerEvent, IState toState)
        {
            FromID = from;
            
            ToID = to;
            
            TriggerEvent = triggerEvent;
            
            ToState = toState;
        }
    }
}