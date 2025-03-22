using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface ITreeState : IState
    {
        public bool IsRootState { get; }
        public ITreeState ParentState { get; set; }
        public ITreeState GetRootState();
        public ITreeState AddSubState(ITreeState subState);
        public ITreeState RemoveSubState(ITreeState subState);
        public bool TryGetSubStates(out List<ITreeState> subStateList);
        public bool TryGetAllSubStates(out List<ITreeState> subStateList);
        public bool TryGetActiveSubState(out ITreeState activeState);
        public bool TryGetActiveSubStates(out List<ITreeState> activeState);
        public ITreeState OnInit(Action act);
        public ITreeState OnEnter(Action act);
        public ITreeState OnUpdate(Action<float> act);
        public ITreeState OnFixedUpdate(Action<float> act);
        public ITreeState OnLateUpdate(Action<float> act);
        public ITreeState OnExit(Action act);
    }
    
    public sealed class TreeState : StateBase, ITreeState
    {
        private readonly IDictionary<string, List<ITreeState>> _subStates = new Dictionary<string, List<ITreeState>>();
        public ITreeState ParentState { get; set; }
        public override string StateID { get; }

        public bool IsRootState => ParentState == null;
        
        private event Action OnInitState;
        
        private event Action OnEnterState;
        
        private event Action<float> OnUpdateState;
        
        private event Action<float> OnFixedUpdateState;
        
        private event Action<float> OnLateUpdateState;
        
        private event Action OnExitState;

        public TreeState(string stateID)
        {
            if (_subStates.TryGetValue(stateID, out var states)) return;
            
            StateID = stateID;
            
            states = new List<ITreeState>();
            
            _subStates[stateID] = states;
        }

        public ITreeState AddSubState(ITreeState subState)
        {
            if (!HasInit)
            {
                Debug.LogError($"You need to init root state first to add Sub-state to : '{StateID}' state!");
                return this;
            }

            if (!_subStates.TryGetValue(StateID, out var states)) return this;

            if (states.Contains(subState))
            {
                Debug.LogError($"{subState.StateID} state has already added to : '{StateID}' state!");
                return this;
            }
            
            subState.ParentState = this;
            
            subState.Init();
            
            states.Add(subState);

            return this;
        }
        
        public ITreeState RemoveSubState(ITreeState subState)
        {
            if (_subStates.ContainsKey(StateID))
            {
                _subStates.Remove(StateID);
            }
            else
            {
                Debug.LogError($"Cannot remove because {StateID} sub-state id not exist!");
            }
            
            return this;
        }

        public bool TryGetActiveSubState(out ITreeState activeState)
        {
            activeState = default;
            
            if (!_subStates.TryGetValue(StateID, out var subStateList)) return false;

            var state = subStateList.FirstOrDefault(x => x.IsActive);
            
            if (state == null)
            {
                Debug.LogError($"{StateID} has no active sub state!");
                return false;
            }

            activeState = state;
            
            return true;
        }

        public bool TryGetActiveSubStates(out List<ITreeState> activeState)
        {
            activeState = default;
            
            if (!_subStates.TryGetValue(StateID, out var subStateList)) return false;

            var state = subStateList.Where(x => x.IsActive).ToList();

            activeState = state;
            
            return true;
        }

        public ITreeState GetRootState()
        {
            ITreeState current = this;

            while (current.ParentState != null)
            {
                current = current.ParentState;
            }

            return current;
        }

        public bool TryGetSubStates(out List<ITreeState> subStateList) 
        {
            subStateList = new List<ITreeState>();
            
            if (!_subStates.TryGetValue(StateID, out var subStates)) return false;
            
            foreach (var subState in subStates)
            {
                subStateList.Add(subState);
            }
            
            return subStateList.Count > 0;
        }
        
        public bool TryGetAllSubStates(out List<ITreeState> subStateList)
        {
            subStateList = new List<ITreeState>();

            if (!_subStates.TryGetValue(StateID, out var directSubStates)) return false;
            
            var stateQueue = new Queue<ITreeState>();
            
            foreach (var subState in directSubStates)
            {
                subStateList.Add(subState);
                
                stateQueue.Enqueue(subState);
            }
            
            while (stateQueue.Count > 0)
            {
                var currentState = stateQueue.Dequeue();

                if (currentState is not TreeState stateBase) continue;
                
                foreach (var subStatePair in stateBase._subStates)
                {
                    foreach (var subState in subStatePair.Value)
                    {
                        subStateList.Add(subState);
                        
                        stateQueue.Enqueue(subState);
                    }
                }
            }

            return subStateList.Count > 0;
        }

        protected override void OnInit() => OnInitState?.Invoke();
        protected override void OnEnter() => OnEnterState?.Invoke();
        protected override void OnUpdate(float deltaTime) => OnUpdateState?.Invoke(deltaTime);
        protected override void OnFixedUpdate(float deltaTime) => OnFixedUpdateState?.Invoke(deltaTime);
        protected override void OnLateUpdate(float deltaTime) => OnLateUpdateState?.Invoke(deltaTime);
        protected override void OnExit() => OnExitState?.Invoke();

        public ITreeState OnInit(Action act)
        {
            OnInitState = act;
            return this;
        }

        public ITreeState OnEnter(Action act)
        {
            OnEnterState = act;
            return this;
        }

        public ITreeState OnUpdate(Action<float> act)
        {
            OnUpdateState = act;
            return this;
        }

        public ITreeState OnFixedUpdate(Action<float> act)
        {
            OnFixedUpdateState = act;
            return this;
        }

        public ITreeState OnLateUpdate(Action<float> act)
        {
            OnLateUpdateState = act;
            return this;
        }

        public ITreeState OnExit(Action act)
        {
            OnExitState = act;
            return this;
        }
    }
}