using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachineBase
    {
        void Tick(float deltaTime);
        void FixedTick(float deltaTime);
        void LateTick(float deltaTime);
    }

    public interface IGlobalStateMachine : IStateMachineBase
    {
        public string CurrentStateID { get; }
        public IReadOnlyList<ITreeState> ActiveStates { get; }
        public bool TryGetRootState(string id, out ITreeState treeState);
        public void AddRootState(string id, ITreeState treeState);
        public bool TryGetSubState(string rootID, string targetStateID, out ITreeState result);
        public bool TryGetAllSubStates(string rootID, out List<ITreeState> result);
        public bool TryAddSubStateToChild(string rootID, string childStateID, ITreeState newSubState);
        public bool TryAddSubStateChain(string rootID, params string[] chain);
        public void AddTransition(string from, string to, Func<bool> condition);
        public void SetInitialState(string targetStateID);
    }
    
    public class GlobalStateMachine : IGlobalStateMachine, ITickable, IFixedTickable, ILateTickable
    {
        private readonly Dictionary<string, ITreeState> _states = new();
        
        private readonly List<ITransition> _transitions = new();
        
        private readonly List<ITreeState> _activeStates = new();

        public IReadOnlyList<ITreeState> ActiveStates => _activeStates;
        public string CurrentStateID { get; private set; }

        public bool TryGetRootState(string id, out ITreeState treeState) => _states.TryGetValue(id, out treeState);

        public void AddRootState(string id, ITreeState treeState)
        {
            if (!_states.TryAdd(id, treeState))
            {
                Debug.LogError($"State with ID '{id}' is already added.");
                return;
            }

            treeState.Init();
        }

        public bool TryGetSubState(string rootID, string targetStateID, out ITreeState result)
        {
            result = null;
            return _states.TryGetValue(rootID, out var rootTree) && rootTree.TryGetStateInChildren(targetStateID, out result);
        }

        public bool TryGetAllSubStates(string rootID, out List<ITreeState> result)
        {
            result = null;
            return _states.TryGetValue(rootID, out var rootTree) && rootTree.TryGetAllStatesInChildren(out result);
        }

        public bool TryAddSubStateToChild(string rootID, string childStateID, ITreeState newSubState)
        {
            if (!_states.TryGetValue(rootID, out var rootTree)) return false;

            if (!rootTree.TryGetStateInChildren(childStateID, out var childState)) return false;

            childState.AddSubState(newSubState);
            
            return true;
        }

        public bool TryAddSubStateChain(string rootID, params string[] chain)
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

        public void AddTransition(string from, string to, Func<bool> condition)
        {
            if (!_states.TryGetValue(from, out var fromState) || !_states.TryGetValue(to, out var toState))
            {
                Debug.LogError($"Cannot create transition from {from} to {to}. States not found.");
                return;
            }

            _transitions.Add(new Transition(fromState, toState, condition));
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
                if (transition.CheckTrigger())
                {
                    transition.To.Enter();

                    if (transition.To is ITreeState newTree)
                    {
                        _activeStates.Clear();
                        _activeStates.AddRange(newTree.GetParentChain(newTree).FindAll(x => x.IsActive));
                    }

                    CurrentStateID = transition.To.StateID;
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

        public void Tick() => Tick(Time.deltaTime);
        public void FixedTick() => FixedTick(Time.fixedDeltaTime);
        public void LateTick() => LateTick(Time.deltaTime);
    }
}