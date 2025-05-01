using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface ITreeState : IState
    {
        public bool IsRootState { get; }
        public ITreeState AddSubState(ITreeState subState);
        public ITreeState RemoveSubState(ITreeState subState);
        public ITreeState GetRootState();
        public List<ITreeState> GetParentChain(ITreeState state);
        public List<ITreeState> GetPathToAncestor(ITreeState from, ITreeState ancestor);
        public bool TryGetStateInParent(string stateID, out ITreeState subState);
        public bool TryGetStateInChildren(string stateID, out ITreeState subState);
        public bool TryGetParentAtDepthLevel(int depthLevel, out ITreeState subState);
        public bool TryGetStatesInChildren(out List<ITreeState> subStateList);
        public bool TryGetAllStatesInChildren(out List<ITreeState> subStateList);
        public bool TryGetActiveChild(out ITreeState activeState);
        public bool TryGetActiveChildren(out List<ITreeState> activeStates);
        public bool HasMoreThanOneActiveChildren();
        public bool TryGetStatePathList(string fromID, string toID, out List<ITreeState> list, bool exceptFirst = false);
        public bool TryExitIfNoActiveSiblings();
        public bool TryExitChain();
        public ITreeState GetParentState();
        protected internal void SetParentState(ITreeState value);
        public int GetDepthLevel();
        protected internal void SetDepthLevel(int value);
    }

    public class TreeState : StateBase, ITreeState
    {
        private readonly Dictionary<string, ITreeState> _subStateLookup = new();
        
        private readonly List<ITreeState> _subStates = new();

        private ITreeState ParentState { get; set; }
        public override string StateID { get; }
        private int DepthLevel { get; set; }
        public bool IsRootState => ParentState == null;

        private event Action OnInitState;
        private event Action OnBeforeEnterState;
        private event Action OnEnterState;
        private event Action<float> OnUpdateState;
        private event Action<float> OnFixedUpdateState;
        private event Action<float> OnLateUpdateState;
        private event Action OnAllExitsComplete;
        private event Action OnExitState;

        public TreeState(string stateID) => StateID = stateID;

        public ITreeState AddSubState(ITreeState subState)
        {
            if (_subStateLookup.ContainsKey(subState.StateID))
            {
                Debug.LogError($"State with ID '{subState.StateID}' already exists under '{StateID}'.");
                return this;
            }

            subState.SetParentState(this);
            
            subState.SetDepthLevel(DepthLevel + 1);
            
            subState.Init();
            
            _subStates.Add(subState);
            
            _subStateLookup[subState.StateID] = subState;
            
            return subState;
        }

        public ITreeState RemoveSubState(ITreeState subState)
        {
            _subStates.Remove(subState);
            
            _subStateLookup.Remove(subState.StateID);
            
            return this;
        }

        public bool TryGetStateInParent(string stateID, out ITreeState subState)
        {
            subState = null;

            var parent = ParentState;
            
            while (parent != null)
            {
                if (parent.StateID == stateID)
                {
                    subState = parent;
                    
                    return true;
                }
                
                parent = parent.GetParentState();
            }

            return false;
        }

        public bool TryGetStateInChildren(string stateID, out ITreeState subState) => _subStateLookup.TryGetValue(stateID, out subState);

        public bool TryGetParentAtDepthLevel(int depthLevel, out ITreeState subState)
        {
            subState = this;

            if (DepthLevel < depthLevel) return false;

            while (subState.GetDepthLevel() > depthLevel)
            {
                subState = subState.GetParentState();
            }

            return true;
        }

        public bool TryGetActiveChild(out ITreeState activeState)
        {
            activeState = _subStates.FirstOrDefault(s => s.IsActive);
            
            return activeState != null;
        }

        public bool TryGetActiveChildren(out List<ITreeState> activeStates)
        {
            activeStates = _subStates.Where(s => s.IsActive).ToList();
            
            return activeStates.Count > 0;
        }

        public bool HasMoreThanOneActiveChildren() => _subStates.Count(s => s.IsActive) > 1;

        public bool TryExitChain()
        {
            foreach (var child in _subStates.ToList())
            {
                if (child.IsActive)
                {
                    child.TryExitChain();
                }
            }

            if (IsActive)
            {
                PerformExit();
            }

            return TryExitIfNoActiveSiblings();
        }
        
        public bool TryExitChainIterative()
        {
            var stack = new Stack<ITreeState>();
            
            stack.Push(this);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is TreeState tree)
                {
                    for (int i = 0; i < tree._subStates.Count; i++)
                    {
                        var child = tree._subStates[i];
                        
                        if (child.IsActive)
                        {
                            stack.Push(child);
                        }
                    }
                }

                if (current.IsActive && current is TreeState concreteState)
                {
                    concreteState.PerformExit();
                }
            }

            return TryExitIfNoActiveSiblings();
        }

        public bool TryExitIfNoActiveSiblings()
        {
            if (ParentState is not TreeState parentTree) return false;

            var hasOtherActive = parentTree._subStates.Any(s => s != this && s.IsActive);

            if (hasOtherActive || !parentTree.IsActive) return false;

            parentTree.PerformExit();
            
            return parentTree.TryExitIfNoActiveSiblings();
        }

        public ITreeState GetParentState() => ParentState;
        void ITreeState.SetParentState(ITreeState value) => ParentState = value;
        public int GetDepthLevel() => DepthLevel;
        void ITreeState.SetDepthLevel(int value) => DepthLevel = value;

        public ITreeState GetRootState()
        {
            ITreeState current = this;

            while (current.GetParentState() != null)
            {
                current = current.GetParentState();
            }

            return current;
        }

        public bool TryGetStatesInChildren(out List<ITreeState> subStateList)
        {
            subStateList = new List<ITreeState>(_subStates);
            
            return subStateList.Count > 0;
        }

        public bool TryGetAllStatesInChildren(out List<ITreeState> allStates)
        {
            var visited = new HashSet<ITreeState>();
            
            var queue = new Queue<ITreeState>(_subStates);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                
                if (!visited.Add(state)) continue;

                if (state is not TreeState tree) continue;
                
                foreach (var sub in tree._subStates)
                {
                    queue.Enqueue(sub);
                }
            }

            allStates = visited.ToList();
            
            return allStates.Count > 0;
        }
        
        public List<ITreeState> GetParentChain(ITreeState state)
        {
            var list = new List<ITreeState>();

            while (state != null)
            {
                list.Add(state);
                
                state = state.GetParentState();
            }

            return list;
        }

        public List<ITreeState> GetPathToAncestor(ITreeState from, ITreeState ancestor)
        {
            var list = new List<ITreeState>();

            var current = from;

            while (current != null)
            {
                list.Add(current);

                if (current == ancestor) break;

                current = current.GetParentState();
            }

            return list;
        }

        public bool TryGetStatePathList(string fromID, string toID, out List<ITreeState> list, bool exceptFirst = false)
        {
            list = new List<ITreeState>();

            if (!GetRootState().TryGetAllStatesInChildren(out var allStates)) return false;

            var from = allStates.FirstOrDefault(s => s.StateID == fromID);
            var to = allStates.FirstOrDefault(s => s.StateID == toID);

            if (from == null || to == null) return false;

            var fromChain = GetParentChain(from);
            var toChain = GetParentChain(to);
            var common = fromChain.Intersect(toChain).FirstOrDefault();

            if (common == null) return false;

            if (from == to)
            {
                list.Add(from);
                return true;
            }

            var a = GetPathToAncestor(from, common);
            var b = GetPathToAncestor(to, common);

            if (exceptFirst && a.Count > 0) a.RemoveAt(0);

            b.Reverse();
            list.AddRange(a);
            list.AddRange(b.Skip(1));
            list = list.OrderBy(x => x.GetDepthLevel()).ToList();

            return true;
        }

        protected override void OnInit() => OnInitState?.Invoke();

        protected override bool OnBeforeEnter()
        {
            var chain = GetParentChain(this).OrderBy(x => x.GetDepthLevel()).Where(x => !x.IsActive).ToList();

            chain.Remove(this);
          
            foreach (var node in chain)
            {
                node.Enter();
            }

            OnBeforeEnterState?.Invoke();
            
            return true;
        }

        protected override void OnEnter() => OnEnterState?.Invoke();
        protected override void OnUpdate(float dt) => OnUpdateState?.Invoke(dt);
        protected override void OnFixedUpdate(float dt) => OnFixedUpdateState?.Invoke(dt);
        protected override void OnLateUpdate(float dt) => OnLateUpdateState?.Invoke(dt);

        public override bool Exit()
        {
            if (!IsActive) return false;

            var success = TryExitChain();

            if (success)
            {
                OnAllExitsComplete?.Invoke();
            }

            return success;
        }

        protected override void OnExit() => OnExitState?.Invoke();
        public ITreeState OnInit(Action act) { OnInitState = act; return this; }
        public ITreeState OnBeforeEnter(Action act) { OnBeforeEnterState = act; return this; }
        public ITreeState OnEnter(Action act) { OnEnterState = act; return this; }
        public ITreeState OnUpdate(Action<float> act) { OnUpdateState = act; return this; }
        public ITreeState OnFixedUpdate(Action<float> act) { OnFixedUpdateState = act; return this; }
        public ITreeState OnLateUpdate(Action<float> act) { OnLateUpdateState = act; return this; }
        public ITreeState OnExit(Action act) { OnExitState = act; return this; }
        public ITreeState OnAllExit(Action act) { OnAllExitsComplete = act; return this; }
    }
}