using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface IState
    {
        public IState ParentState { get; set; }
        public string StateID { get; }
        public bool HasInit { get; }
        public bool IsActive { get; }
        public bool IsRootState { get; }
        public bool NeedsExitTime { get; }
        public bool IsGhostState { get; }
        public void Init();
        public void Enter();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
        public void Exit();
        public IState AddSubState(IState subState);
        public IState RemoveSubState(IState subState);
        public void ChangeSubState(IState nextSubState);
        public IState GetRootState();
        public bool TryGetAllSubStates(out List<IState> subStateList);
        public bool TryGetSubStates(out List<IState> subStateList);
        public bool TryGetActiveSubState(out IState activeState);
        public bool TryGetActiveSubStates(out List<IState> activeState);
    }
    
    public sealed class StateBase : IState
    {
        private readonly IDictionary<string, List<IState>> _subStates = new Dictionary<string, List<IState>>();
        public IState ParentState { get; set; }
        public bool HasInit { get; private set; }
        public bool IsActive { get; private set; }
        public string StateID { get; }
        public bool IsRootState { get; }
        public bool NeedsExitTime { get; }
        public bool IsGhostState { get; }

        public StateBase(string stateID, bool isRootState, bool needsExitTime, bool isGhostState = false)
        {
            if (!_subStates.TryGetValue(stateID, out var states))
            {
                StateID = stateID;
                states = new List<IState>();
                _subStates[stateID] = states;
            }
            
            IsRootState = isRootState;
            NeedsExitTime = needsExitTime;
            IsGhostState = isGhostState;
        }

        public IState AddSubState(IState subState)
        {
            if (!_subStates.TryGetValue(StateID, out var states) || states.Contains(subState)) return null;
            
            subState.Init();
            subState.ParentState = this;
            states.Add(subState);

            return subState;
        }
        
        public IState RemoveSubState(IState subState)
        {
            if (_subStates.ContainsKey(StateID))
            {
                _subStates.Remove(StateID);
            }
            else
            {
                Debug.LogError($"Cannot remove because {StateID} sub-state id has already removed!");
            }
            
            return this;
        }

        public bool TryGetParentState(out IState parentState)
        {
            if (ParentState != null)
            {
                parentState = ParentState;
                return true;
            }

            parentState = default;
            return false;
        }

        public void Init()
        {
            HasInit = true;
        }

        public void Enter()
        {
            IsActive = true;
        }

        public void Update(float deltaTime) { }
        
        public void FixedUpdate(float deltaTime) { }

        public void LateUpdate(float deltaTime) { }

        public void Exit()
        {
            IsActive = false;
        }

        public void ChangeSubState(IState nextSubState)
        {
            if (!_subStates.TryGetValue(StateID, out var subStateList))
            {
                Debug.LogError($"Cannot change because {StateID} sub-state id is not exist!");
                return;
            }

            var activeSubState = subStateList.FirstOrDefault(x => x.IsActive);

            if (activeSubState == null)
            {
                Debug.LogError($"{StateID} has no active sub state!");
                return;
            }
            
            activeSubState.Exit();

            var selectedNextSubState = subStateList.FirstOrDefault(x => x.Equals(nextSubState));

            if (selectedNextSubState != null)
            {
                selectedNextSubState.Enter();
            }
            else
            {
                Debug.LogError($"{nextSubState.StateID} is not exist in the sub list!");
            }
        }

        public bool TryGetActiveSubState(out IState activeState)
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

        public bool TryGetActiveSubStates(out List<IState> activeState)
        {
            activeState = default;
            
            if (!_subStates.TryGetValue(StateID, out var subStateList)) return false;

            var state = subStateList.Where(x => x.IsActive).ToList();

            activeState = state;
            
            return true;
        }

        public IState GetRootState()
        {
            if (!TryGetParentState(out var parentState)) return this;
            
            while (parentState.ParentState != null)
            {
                parentState = parentState.ParentState;
            }
                
            return parentState;
        }

        public bool TryGetSubStates(out List<IState> subStateList) 
        {
            subStateList = new List<IState>();
            
            if (!_subStates.TryGetValue(StateID, out var subStates)) return false;
            
            foreach (var subState in subStates)
            {
                subStateList.Add(subState);
            }
            
            return subStateList.Count > 0;
        }
        
        public bool TryGetAllSubStates(out List<IState> subStateList)
        {
            subStateList = new List<IState>();

            if (!_subStates.TryGetValue(StateID, out var directSubStates)) return false;
            
            Queue<IState> stateQueue = new Queue<IState>();
            
            foreach (var subState in directSubStates)
            {
                subStateList.Add(subState);
                
                stateQueue.Enqueue(subState);
            }
            
            while (stateQueue.Count > 0)
            {
                IState currentState = stateQueue.Dequeue();

                if (currentState is not StateBase stateBase) continue;
                
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
    }
}