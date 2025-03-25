using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachine<TState> : IStateMachineBase
    {
        public TState CurrentStateID { get; }
        public IReadOnlyList<ITreeState> ActiveStates { get; }
        public bool TryGetRootState(TState id, out ITreeState treeState);
        public void AddRootState(TState id, ITreeState treeState);
        public bool TryGetSubState(TState rootID, string targetStateID, out ITreeState result);
        public bool TryGetAllSubStates(TState rootID, out List<ITreeState> result);
        public bool TryAddSubStateToChild(TState rootID, string childStateID, ITreeState newSubState);
        public bool TryAddSubStateChain(TState rootID, params string[] chain);
        public void AddTransition(TState from, TState to, Func<bool> condition);
        public void SetInitialState(string targetStateID);
    }

    public interface IStateMachine<TState, in TEvent> : IStateMachine<TState>
    {
        public void AddEventTransition(TState from, TState to, TEvent evt);
        public void HandleEvent(TEvent evt);
    }

    public class StateMachine<TState> : IStateMachine<TState>
    {
        protected readonly Dictionary<TState, ITreeState> _states = new();
        
        protected readonly List<IGenericTransition<TState>> _transitions = new();
        
        protected readonly List<ITreeState> _activeStates = new();

        public IReadOnlyList<ITreeState> ActiveStates => _activeStates;
        public TState CurrentStateID { get; protected set; }

        public bool TryGetRootState(TState id, out ITreeState treeState) => _states.TryGetValue(id, out treeState);

        public void AddRootState(TState id, ITreeState treeState)
        {
            if (!_states.TryAdd(id, treeState))
            {
                Debug.LogError($"State with ID '{id}' is already added.");
                return;
            }
            
            treeState.Init();
        }

        public bool TryGetSubState(TState rootID, string targetStateID, out ITreeState result)
        {
            result = null;
            
            return _states.TryGetValue(rootID, out var rootTree) && rootTree.TryGetStateInChildren(targetStateID, out result);
        }

        public bool TryGetAllSubStates(TState rootID, out List<ITreeState> result)
        {
            result = null;
            
            return _states.TryGetValue(rootID, out var rootTree) && rootTree.TryGetAllStatesInChildren(out result);
        }

        public bool TryAddSubStateToChild(TState rootID, string childStateID, ITreeState newSubState)
        {
            if (!_states.TryGetValue(rootID, out var rootTree)) return false;
            
            if (!rootTree.TryGetStateInChildren(childStateID, out var childState)) return false;
            
            childState.AddSubState(newSubState);
            
            return true;
        }

        public bool TryAddSubStateChain(TState rootID, params string[] chain)
        {
            if (chain == null || chain.Length < 2) return false;
            
            if (!_states.TryGetValue(rootID, out var root)) return false;

            ITreeState current = root;
            
            foreach (var id in chain)
            {
                if (!current.TryGetStateInChildren(id, out var found))
                {
                    found = new TreeState(id);
                    
                    current.AddSubState(found);
                }
                
                current = found;
            }

            return true;
        }

        public void AddTransition(TState from, TState to, Func<bool> condition)
        {
            if (!_states.TryGetValue(from, out var fromState) || !_states.TryGetValue(to, out var toState))
            {
                Debug.LogError($"Cannot create transition from {from} to {to}. States not found.");
                return;
            }
            
            _transitions.Add(new GenericTransition<TState>(from, to, fromState, toState, condition));
        }

        public void SetInitialState(string targetStateID)
        {
            foreach (var (id, state) in _states)
            {
                if (state.StateID == targetStateID)
                {
                    state.Enter();
                    
                    _activeStates.Clear();
                    
                    _activeStates.AddRange(state.GetParentChain(state).FindAll(x => x.IsActive));
                    
                    CurrentStateID = id;
                    
                    return;
                }
            }
            Debug.LogError($"Initial state with ID '{targetStateID}' not found in any root state.");
        }

        public void Tick(float deltaTime)
        {
            foreach (var state in _activeStates)
            {
                state.Update(deltaTime);
            }

            foreach (var transition in _transitions)
            {
                if (EqualityComparer<TState>.Default.Equals(CurrentStateID, transition.FromID) && transition.Check())
                {
                    transition.ToState.Enter();
                    
                    if (transition.ToState is ITreeState newTree)
                    {
                        _activeStates.Clear();
                        
                        _activeStates.AddRange(newTree.GetParentChain(newTree).FindAll(x => x.IsActive));
                    }
                    
                    CurrentStateID = transition.ToID;
                    break;
                }
            }
        }

        public void FixedTick(float deltaTime)
        {
            foreach (var state in _activeStates)
            {
                state.FixedUpdate(deltaTime);
            }
        }

        public void LateTick(float deltaTime)
        {
            foreach (var state in _activeStates)
            {
                state.LateUpdate(deltaTime);
            }
        }
    }

    public class StateMachine<TState, TEvent> : StateMachine<TState>, IStateMachine<TState, TEvent>
    {
        private readonly List<IGenericEventTransition<TState, TEvent>> _eventTransitions = new();

        public void AddEventTransition(TState from, TState to, TEvent evt)
        {
            if (!_states.TryGetValue(to, out var toState))
            {
                Debug.LogError($"Cannot create event transition to {to}. State not found.");
                return;
            }
            
            _eventTransitions.Add(new GenericEventTransition<TState, TEvent>(from, to, evt, toState));
        }

        public void HandleEvent(TEvent evt)
        {
            foreach (var transition in _eventTransitions)
            {
                if (EqualityComparer<TState>.Default.Equals(CurrentStateID, transition.FromID) &&
                    EqualityComparer<TEvent>.Default.Equals(evt, transition.TriggerEvent))
                {
                    transition.ToState.Enter();
                    
                    if (transition.ToState is ITreeState newTree)
                    {
                        _activeStates.Clear();
                        
                        _activeStates.AddRange(newTree.GetParentChain(newTree).FindAll(x => x.IsActive));
                    }
                    
                    CurrentStateID = transition.ToID;
                    
                    break;
                }
            }
        }
    }
}