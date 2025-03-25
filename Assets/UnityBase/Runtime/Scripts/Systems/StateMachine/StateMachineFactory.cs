using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachineFactory
    {
        IStateMachine<TState, TEvent> Create<TState, TEvent>(bool registerTick = true);
        IStateMachine<TState> Create<TState>(bool registerTick = true);
        bool TryGet<TState, TEvent>(out IStateMachine<TState, TEvent> fsm);
        bool TryGet<TState>(out IStateMachine<TState> fsm);
        bool Remove<TState, TEvent>();
        bool Remove<TState>();
        void TickAll(float deltaTime);
        void FixedTickAll(float deltaTime);
        void LateTickAll(float deltaTime);
    }

    public class StateMachineFactory : IStateMachineFactory, ITickable, IFixedTickable, ILateTickable
    {
        private readonly Dictionary<Type, IStateMachineBase> _fsmMap = new();
        
        private readonly object _lock = new();

        public IStateMachine<TState, TEvent> Create<TState, TEvent>(bool registerTick = true)
        {
            var key = typeof((TState, TEvent)); 

            lock (_lock)
            {
                if (_fsmMap.TryGetValue(key, out var existing))
                {
                    return (IStateMachine<TState, TEvent>)existing;
                }

                var fsm = new StateMachine<TState, TEvent>();

                if (registerTick)
                {
                    _fsmMap[key] = fsm;
                }

                return fsm;
            }
        }

        public IStateMachine<TState> Create<TState>(bool registerTick = true)
        {
            var key = typeof(TState);

            lock (_lock)
            {
                if (_fsmMap.TryGetValue(key, out var existing))
                {
                    return (IStateMachine<TState>)existing;
                }

                var fsm = new StateMachine<TState>();

                if (registerTick)
                {
                    _fsmMap[key] = fsm;
                }

                return fsm;
            }
        }

        public bool TryGet<TState, TEvent>(out IStateMachine<TState, TEvent> fsm)
        {
            var key = typeof((TState, TEvent));
            
            return TryGetInternal(key, out fsm);
        }

        public bool TryGet<TState>(out IStateMachine<TState> fsm)
        {
            var key = typeof(TState);
            
            return TryGetInternal(key, out fsm);
        }

        private bool TryGetInternal<T>(Type key, out T fsm) where T : class
        {
            lock (_lock)
            {
                if (_fsmMap.TryGetValue(key, out var obj) && obj is T casted)
                {
                    fsm = casted;
                    return true;
                }

                fsm = null;
                return false;
            }
        }

        public bool Remove<TState, TEvent>()
        {
            var key = typeof((TState, TEvent));
            return RemoveInternal(key);
        }

        public bool Remove<TState>()
        {
            var key = typeof(TState);
            return RemoveInternal(key);
        }

        private bool RemoveInternal(Type key)
        {
            lock (_lock)
            {
                return _fsmMap.Remove(key);
            }
        }

        public void TickAll(float deltaTime)
        {
            lock (_lock)
            {
                foreach (var fsm in _fsmMap.Values)
                {
                    fsm.Tick(deltaTime);
                }
            }
        }

        public void FixedTickAll(float deltaTime)
        {
            lock (_lock)
            {
                foreach (var fsm in _fsmMap.Values)
                {
                    fsm.FixedTick(deltaTime);
                }
            }
        }

        public void LateTickAll(float deltaTime)
        {
            lock (_lock)
            {
                foreach (var fsm in _fsmMap.Values)
                {
                    fsm.LateTick(deltaTime);
                }
            }
        }

        public void Tick() => TickAll(Time.deltaTime);
        
        public void FixedTick() => FixedTickAll(Time.fixedDeltaTime);
        
        public void LateTick() => LateTickAll(Time.deltaTime);
    }
}