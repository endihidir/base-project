using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityBase.StateMachineCore
{
    public interface IStateNode : IState
    {
        public bool IsRootState { get; }
        public event Action OnInitState;
        public event Action OnBeforeEnterState;
        public event Action OnEnterState;
        public event Action<float> OnUpdateState;
        public event Action<float> OnFixedUpdateState;
        public event Action<float> OnLateUpdateState;
        public event Action OnExitState;
        public Func<IStateNode, bool> ResolveNeedsExitTime { get; set; }
        public event Action<IStateNode, Action> HandleExitRequest;
        public void MarkExitReady();
        public IStateNode AddSubState(IStateNode subStateNode);
        public IStateNode RemoveSubState(IStateNode subStateNode);
        public IStateNode AddSubState(string subStateID = null);
        public IStateNode GetRootState();
        public List<IStateNode> GetParentChain(IStateNode stateNode);
        public List<IStateNode> GetPathToAncestor(IStateNode from, IStateNode ancestor);
        public bool TryGetStateInParent(string stateID, out IStateNode subStateNode);
        public bool TryGetStateInChildren(string stateID, out IStateNode subStateNode);
        public bool TryGetParentAtDepthLevel(int depthLevel, out IStateNode subStateNode);
        public bool TryGetStatesInChildren(out List<IStateNode> subStateList);
        public bool TryGetAllStatesInChildren(out List<IStateNode> subStateList, bool addSelf = false);
        public bool TryGetFirstActiveChild(out IStateNode activeStateNode);
        public bool TryGetActiveChildren(out List<IStateNode> activeStates);
        public bool HasMoreThanOneActiveChild();
        public bool TryGetStatePathList(string fromID, string toID, out List<IStateNode> list, bool exceptFirst = false);
        public bool CollapseEmptyAncestors();
        public bool TryExitChain();
        public IStateNode GetParentState();
        protected internal void SetParentState(IStateNode value);
        public int GetDepthLevel();
        protected internal void SetDepthLevel(int value);
        public IStateNode ClearAllListeners();
    }

    public sealed class StateNode : StateBase, IStateNode
    {
        private readonly Dictionary<string, IStateNode> _subStateLookup = new();
        private readonly List<IStateNode> _subStates = new();

        private IStateNode ParentStateNode { get; set; }
        private int DepthLevel { get; set; }
        public bool IsRootState => ParentStateNode == null;
        public event Action OnInitState;
        public event Action OnBeforeEnterState;
        public event Action OnEnterState;
        public event Action<float> OnUpdateState;
        public event Action<float> OnFixedUpdateState;
        public event Action<float> OnLateUpdateState;
        public event Action OnExitState;
        public event Action<IStateNode, Action> HandleExitRequest;
        public Func<IStateNode, bool> ResolveNeedsExitTime { get; set; }

        private static int _rootCounter;
        
        private static readonly Dictionary<string, int> PerRootChildCounters = new();
        
        public StateNode(string stateID = null)
        {
            StateID = stateID;
        }
        
        public override bool NeedsExitTime => ResolveNeedsExitTime?.Invoke(this) ?? base.NeedsExitTime;
        public void MarkExitReady() => base.RequestExit();

        public override void RequestExit()
        {
            if (NeedsExitTime && HandleExitRequest != null)
            {
                try
                {
                    HandleExitRequest.Invoke(this, () => base.RequestExit());
                }
                catch (Exception e)
                {
                    if (ShowLogs) DebugLogger.LogError($"HandleExitRequest exception in '{StateID}': {e}");
                    base.RequestExit();
                }
                return;
            }

            base.RequestExit();
        }

        public IStateNode AddSubState(IStateNode subStateNode)
        {
            if (!string.IsNullOrEmpty(subStateNode.StateID) && _subStateLookup.ContainsKey(subStateNode.StateID))
            {
                if (ShowLogs)
                    DebugLogger.LogError($"State with ID '{subStateNode.StateID}' already exists under '{StateID}'.");
                return _subStateLookup[subStateNode.StateID];
            }

            subStateNode.SetParentState(this);
            subStateNode.SetDepthLevel(DepthLevel + 1);
            _subStates.Add(subStateNode);
            subStateNode.Init(Blackboard, ShowLogs);
            _subStateLookup[subStateNode.StateID] = subStateNode;
            return subStateNode;
        }

        public IStateNode AddSubState(string subStateID = null)
        {
            return AddSubState(new StateNode(subStateID));
        }

        public IStateNode RemoveSubState(IStateNode subStateNode)
        {
            _subStates.Remove(subStateNode);
            _subStateLookup.Remove(subStateNode.StateID);
            return this;
        }

        public bool TryGetStateInParent(string stateID, out IStateNode subStateNode)
        {
            subStateNode = null;
            var parent = ParentStateNode;

            while (parent != null)
            {
                if (parent.StateID == stateID)
                {
                    subStateNode = parent;
                    return true;
                }

                parent = parent.GetParentState();
            }

            return false;
        }

        public bool TryGetStateInChildren(string stateID, out IStateNode subStateNode) => _subStateLookup.TryGetValue(stateID, out subStateNode);

        public bool TryGetParentAtDepthLevel(int depthLevel, out IStateNode subStateNode)
        {
            subStateNode = this;
            if (DepthLevel < depthLevel) return false;

            while (subStateNode.GetDepthLevel() > depthLevel)
            {
                subStateNode = subStateNode.GetParentState();
            }

            return true;
        }

        public bool TryGetFirstActiveChild(out IStateNode activeStateNode)
        {
            activeStateNode = _subStates.FirstOrDefault(s => s.IsActive);
            return activeStateNode != null;
        }

        public bool TryGetActiveChildren(out List<IStateNode> activeStates)
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
            if (ParentStateNode is not StateNode parentTree) return false;

            var hasOtherActive = parentTree._subStates.Any(s => s != this && s.IsActive);
            if (hasOtherActive || !parentTree.IsActive) return false;

            parentTree.PerformExit();
            return parentTree.CollapseEmptyAncestors();
        }

        public IStateNode GetParentState() => ParentStateNode;
        void IStateNode.SetParentState(IStateNode value) => ParentStateNode = value;
        public int GetDepthLevel() => DepthLevel;
        void IStateNode.SetDepthLevel(int value) => DepthLevel = value;

        public IStateNode GetRootState()
        {
            IStateNode current = this;
            
            while (current.GetParentState() != null)
            {
                current = current.GetParentState();
            }
            
            return current;
        }

        public bool TryGetStatesInChildren(out List<IStateNode> subStateList)
        {
            subStateList = new List<IStateNode>(_subStates);
            
            return subStateList.Count > 0;
        }

        public bool TryGetAllStatesInChildren(out List<IStateNode> allStates, bool addSelf = false)
        {
            var visited = new HashSet<IStateNode>();
            
            if (addSelf) visited.Add(this);

            var queue = new Queue<IStateNode>(_subStates);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();

                if (!visited.Add(state)) continue;

                if (state is not StateNode tree) continue;

                foreach (var sub in tree._subStates)
                {
                    queue.Enqueue(sub);
                }
            }

            allStates = visited.ToList();
            
            return true;
        }

        public List<IStateNode> GetParentChain(IStateNode stateNode)
        {
            var list = new List<IStateNode>();
            
            while (stateNode != null)
            {
                list.Add(stateNode);
                
                stateNode = stateNode.GetParentState();
            }
            
            return list;
        }

        public List<IStateNode> GetPathToAncestor(IStateNode from, IStateNode ancestor)
        {
            var list = new List<IStateNode>();
            
            var current = from;

            while (current != null)
            {
                list.Add(current);
                
                if (current == ancestor) break;
                
                current = current.GetParentState();
            }

            return list;
        }

        public bool TryGetStatePathList(string fromID, string toID, out List<IStateNode> list, bool exceptFirst = false)
        {
            list = new List<IStateNode>();

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

            ParentStateNode = null;
            DepthLevel = 0;
            
            ResolveNeedsExitTime = null;
            HandleExitRequest = null;
        }

        public IStateNode ClearAllListeners()
        {
            OnInitState = null;
            OnBeforeEnterState = null;
            OnEnterState = null;
            OnUpdateState = null;
            OnFixedUpdateState = null;
            OnLateUpdateState = null;
            OnExitState = null;
            return this;
        }
    }
}