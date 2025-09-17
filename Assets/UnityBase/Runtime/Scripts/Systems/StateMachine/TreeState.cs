using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityBase.StateMachineCore
{
    public interface ITreeState : IState
    {
        public bool IsRootState { get; }
        public event Action OnInitState; 
        public event Action OnBeforeEnterState;
        public event Action OnEnterState;
        public event Action<float> OnUpdateState;
        public event Action<float> OnFixedUpdateState;
        public event Action<float> OnLateUpdateState;
        public event Action OnExitState;
        public ITreeState AddSubState(ITreeState subState);
        public ITreeState RemoveSubState(ITreeState subState);
        public ITreeState AddSubState(string subStateID = null); //New
        public ITreeState GetRootState();
        public List<ITreeState> GetParentChain(ITreeState state);
        public List<ITreeState> GetPathToAncestor(ITreeState from, ITreeState ancestor);
        public bool TryGetStateInParent(string stateID, out ITreeState subState);
        public bool TryGetStateInChildren(string stateID, out ITreeState subState);
        public bool TryGetParentAtDepthLevel(int depthLevel, out ITreeState subState);
        public bool TryGetStatesInChildren(out List<ITreeState> subStateList);
        public bool TryGetAllStatesInChildren(out List<ITreeState> subStateList, bool addSelf = false);
        public bool TryGetFirstActiveChild(out ITreeState activeState);
        public bool TryGetActiveChildren(out List<ITreeState> activeStates);
        public bool HasMoreThanOneActiveChild();
        public bool TryGetStatePathList(string fromID, string toID, out List<ITreeState> list, bool exceptFirst = false);
        public bool CollapseEmptyAncestors();
        public bool TryExitChain();
        public ITreeState GetParentState();
        protected internal void SetParentState(ITreeState value);
        public int GetDepthLevel();
        protected internal void SetDepthLevel(int value);
        public ITreeState ClearAllListeners();
    }

    public class TreeState : StateBase, ITreeState
    {
        private readonly Dictionary<string, ITreeState> _subStateLookup = new();

        private readonly List<ITreeState> _subStates = new();

        private ITreeState ParentState { get; set; }
        private int DepthLevel { get; set; }
        public bool IsRootState => ParentState == null;
        public event Action OnInitState;
        public event Action OnBeforeEnterState;
        public event Action OnEnterState;
        public event Action<float> OnUpdateState;
        public event Action<float> OnFixedUpdateState;
        public event Action<float> OnLateUpdateState;
        public event Action OnExitState;

        private static int _rootCounter;

        private static readonly Dictionary<string, int> PerRootChildCounters = new();

        public TreeState(string stateID = null)
        {
            StateID = stateID;
        }

        public ITreeState AddSubState(ITreeState subState)
        {
            if (!string.IsNullOrEmpty(subState.StateID) && _subStateLookup.ContainsKey(subState.StateID))
            {
                if (ShowLogs)
                    DebugLogger.LogError($"State with ID '{subState.StateID}' already exists under '{StateID}'.");
                
                return _subStateLookup[subState.StateID];
            }

            subState.SetParentState(this);

            subState.SetDepthLevel(DepthLevel + 1);

            _subStates.Add(subState);

            subState.Init(Blackboard, ShowLogs);

            _subStateLookup[subState.StateID] = subState;

            return subState;
        }

        public ITreeState AddSubState(string subStateID = null) //New
        {
            return AddSubState(new TreeState(subStateID)); //New
        }

        public ITreeState RemoveSubState(ITreeState subState)
        {
            _subStates.Remove(subState);

            _subStateLookup.Remove(subState.StateID);

            return this;
        }

        public bool TryGetStateInParent(string stateID, out ITreeState subState
        )
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

        public bool TryGetFirstActiveChild(out ITreeState activeState)
        {
            activeState = _subStates.FirstOrDefault(s => s.IsActive);

            return activeState != null;
        }

        public bool TryGetActiveChildren(out List<ITreeState> activeStates)
        {
            activeStates = _subStates.Where(s => s.IsActive).ToList();

            return activeStates.Count > 0;
        }

        public bool HasMoreThanOneActiveChild() => _subStates.Count(s => s.IsActive) > 1;

        public bool TryExitChain()
        {
            foreach (var child in _subStates)
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

            return CollapseEmptyAncestors();
        }

        public bool CollapseEmptyAncestors()
        {
            if (ParentState is not TreeState parentTree) return false;

            var hasOtherActive = parentTree._subStates.Any(s => s != this && s.IsActive);

            if (hasOtherActive || !parentTree.IsActive) return false;

            parentTree.PerformExit();

            return parentTree.CollapseEmptyAncestors();
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

        public bool TryGetAllStatesInChildren(out List<ITreeState> allStates, bool addSelf = false)
        {
            var visited = new HashSet<ITreeState>();

            if (addSelf)
                visited.Add(this);

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

            return true;
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

            if (!GetRootState().TryGetAllStatesInChildren(out var allStates, true)) return false;

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

            return true;
        }

        protected override void OnInit()
        {
            AssignAutoIdIfEmpty();

            OnInitState?.Invoke();
        }

        private void AssignAutoIdIfEmpty()
        {
            if (!string.IsNullOrEmpty(StateID)) return;

            if (IsRootState)
            {
                StateID = $"Root_{++_rootCounter}";
            }
            else
            {
                var root = GetRootState();
                var rootId = root?.StateID ?? "Root";
                var c = PerRootChildCounters.GetValueOrDefault(rootId, 0);
                c++;
                PerRootChildCounters[rootId] = c;
                StateID = $"{rootId}-N_{c}";
            }
        }

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
        protected override void OnExit() => OnExitState?.Invoke();
        public override bool Exit() => IsActive && TryExitChain();
        
        public override void ClearAll()
        {
            base.ClearAll();

            _subStateLookup.Clear();
            _subStates.Clear();

            ClearAllListeners();

            ParentState = null;
            DepthLevel = 0;
        }
        
        public ITreeState ClearAllListeners()
        {
            OnInitState        = null;
            OnBeforeEnterState = null;
            OnEnterState       = null;
            OnUpdateState      = null;
            OnFixedUpdateState = null;
            OnLateUpdateState  = null;
            OnExitState        = null;
            return this;
        }
    }
}
