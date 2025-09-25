using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.BlackboardCore;

namespace UnityBase.StateMachineCore
{
    public interface IHierarchicalStateMachine : IStateMachineBase
    {
        public IReadOnlyList<IStateNode> ActiveBranch { get; }

        public bool TryGetRootState(string id, out IStateNode stateNode);

        public IStateNode CreateState(string stateID = null, IBlackboard defaultBlackboard = null, bool showLogs = true);
        public IStateNode RegisterRoot(IStateNode root);
        public IStateNode EnsureSubStateChain(string rootID, params string[] chain);
        public IStateNode EnsureSubStateChain(IStateNode root, params string[] chain);

        public bool TryFindState(string rootID, string stateID, out IStateNode result);
        public bool TryFindStateByPath(string path, out IStateNode result);

        public IHierarchicalStateMachine SetInitialState(string targetStateID);
        public IHierarchicalStateMachine SetInitialState(string rootID, string targetStateID);
        public IHierarchicalStateMachine SetInitialState(IStateNode target);

        public IHierarchicalStateMachine AddTransition(string from, string to, Func<bool> condition);
        public IHierarchicalStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition);
        public IHierarchicalStateMachine AddTransition(IStateNode from, IStateNode to, Func<bool> condition);

        public IHierarchicalStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot); 
        public IHierarchicalStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition, int priority, bool oneShot); 
        public IHierarchicalStateMachine AddTransition(IStateNode from, IStateNode to, Func<bool> condition, int priority, bool oneShot); 

        public bool TryGetSubState(string rootID, string targetStateID, out IStateNode result);
        public bool TryGetAllSubStates(string rootID, out List<IStateNode> result);
    }

    public class HierarchicalStateMachine : IHierarchicalStateMachine
    {
        private readonly Dictionary<string, IStateNode> _states = new();
        private readonly List<ITransition> _transitions = new();
        private readonly List<IStateNode> _activeBranch = new();
        private readonly Dictionary<string, ITransition> _pendingPerRoot = new(); 

        public IReadOnlyList<IStateNode> ActiveBranch => _activeBranch;
        public string CurrentStateID { get; private set; }

        public bool TryGetRootState(string id, out IStateNode stateNode) => _states.TryGetValue(id, out stateNode);

        public IStateNode CreateState(string stateID = null, IBlackboard defaultBlackboard = null, bool showLogs = true)
        {
            if (stateID != null && _states.TryGetValue(stateID, out var treeState)) return treeState;
            
            if (string.IsNullOrEmpty(stateID))
            {
                var root = new StateNode();
                root.Init(defaultBlackboard, showLogs);
                if (_states.TryGetValue(root.StateID, out var existing)) return existing;
                _states[root.StateID] = root;
                return root;
            }
            else
            {
                if (_states.TryGetValue(stateID, out var existing)) return existing;

                var root = new StateNode(stateID);
                root.Init(defaultBlackboard, showLogs);
                _states[stateID] = root;
                return root;
            }
        }

        public IStateNode RegisterRoot(IStateNode root)
        {
            if (root == null || string.IsNullOrEmpty(root.StateID)) return null;

            if (_states.TryGetValue(root.StateID, out var existing))
            {
                return existing;
            }
            
            _states[root.StateID] = root;
            return root;
        }

        public IStateNode EnsureSubStateChain(string rootID, params string[] chain)
        {
            if (chain == null || chain.Length == 0) return null;
            if (!_states.TryGetValue(rootID, out var root)) return null;

            var current = root;

            for (int i = 0; i < chain.Length; i++)
            {
                var id = chain[i];

                if (!current.TryGetStateInChildren(id, out var found))
                {
                    found = new StateNode(id);
                    current.AddSubState(found);
                }

                current = found;
            }

            return current;
        }

        public IStateNode EnsureSubStateChain(IStateNode root, params string[] chain)
        {
            if (root == null || chain == null || chain.Length == 0) return null;
            if (!TryResolveRoot(root, out _, out var storedRoot)) return null;

            var current = storedRoot;

            for (int i = 0; i < chain.Length; i++)
            {
                var id = chain[i];

                if (!current.TryGetStateInChildren(id, out var found))
                {
                    found = new StateNode(id);
                    current.AddSubState(found);
                }

                current = found;
            }

            return current;
        }

        public bool TryFindState(string rootID, string stateID, out IStateNode result)
        {
            result = null;

            if (!_states.TryGetValue(rootID, out var rootTree)) return false;

            if (rootTree.StateID == stateID)
            {
                result = rootTree;
                return true;
            }

            if (!rootTree.TryGetAllStatesInChildren(out var all, true)) return false;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].StateID == stateID)
                {
                    result = all[i];
                    return true;
                }
            }

            return false;
        }

        public bool TryFindStateByPath(string path, out IStateNode result)
        {
            result = null;
            if (string.IsNullOrEmpty(path)) return false;

            var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            var rootKeyOrId = parts[0];

            if (!_states.TryGetValue(rootKeyOrId, out var current))
            {
                current = _states.Values.FirstOrDefault(r => r.StateID == rootKeyOrId);
                if (current == null) return false;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                var seg = parts[i];

                if (!current.TryGetStateInChildren(seg, out var next))
                {
                    return false;
                }

                current = next;
            }

            result = current;
            return true;
        }

        public IHierarchicalStateMachine SetInitialState(string targetStateID)
        {
            if (!TryFindStateGlobal(targetStateID, out var target))
            {
                DebugLogger.LogError($"Initial state with ID '{targetStateID}' not found in any root state.");
                return null;
            }

            return SetInitialState(target);
        }

        public IHierarchicalStateMachine SetInitialState(string rootID, string targetStateID)
        {
            if (!TryFindState(rootID, targetStateID, out var target))
            {
                DebugLogger.LogError($"Initial state with ID '{targetStateID}' not found under root '{rootID}'.");
                return null;
            }

            return SetInitialState(target);
        }

        public IHierarchicalStateMachine SetInitialState(IStateNode target)
        {
            if (target == null) return null;

            var root = target.GetRootState();

            if (!root.IsActive) root.Enter();

            var stack = new Stack<IStateNode>();

            var n = target;

            while (n != null && n != root)
            {
                stack.Push(n);
                n = n.GetParentState();
            }

            while (stack.Count > 0)
            {
                stack.Pop().Enter();
            }

            _activeBranch.Clear();

            var leaf = target;

            while (leaf != null)
            {
                _activeBranch.Add(leaf);
                leaf = leaf.GetParentState();
            }

            _activeBranch.Reverse();
            CurrentStateID = root.StateID;

            return this;
        }

        public IHierarchicalStateMachine AddTransition(string fromId, string toId, Func<bool> condition)
        {
            if (!TryFindStateGlobal(fromId, out var from) || !TryFindStateGlobal(toId, out var to))
            {
                DebugLogger.LogError($"Cannot create transition from '{fromId}' to '{toId}'. States not found.");
                return null;
            }

            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IHierarchicalStateMachine AddTransition(string rootID, string fromId, string toId, Func<bool> condition)
        {
            if (!TryFindState(rootID, fromId, out var from) || !TryFindState(rootID, toId, out var to))
            {
                DebugLogger.LogError($"Cannot create transition from '{fromId}' to '{toId}' under root '{rootID}'. States not found.");
                return null;
            }

            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IHierarchicalStateMachine AddTransition(IStateNode from, IStateNode to, Func<bool> condition)
        {
            if (from == null || to == null) return this;
            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IHierarchicalStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (!TryFindStateGlobal(from, out var f) || !TryFindStateGlobal(to, out var t))
            {
                DebugLogger.LogError($"Cannot create transition from '{from}' to '{to}'. States not found.");
                return null;
            }
            _transitions.Add(new Transition(f, t, condition, priority, oneShot)); 
            return this; 
        }

        public IHierarchicalStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (!TryFindState(rootID, from, out var f) || !TryFindState(rootID, to, out var t))
            {
                DebugLogger.LogError($"Cannot create transition from '{from}' to '{to}' under root '{rootID}'. States not found.");
                return null;
            }
            _transitions.Add(new Transition(f, t, condition, priority, oneShot)); 
            return this; 
        }

        public IHierarchicalStateMachine AddTransition(IStateNode from, IStateNode to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (from == null || to == null) return this; 
            _transitions.Add(new Transition(from, to, condition, priority, oneShot)); 
            return this; 
        }

        public bool TryGetSubState(string rootID, string targetStateID, out IStateNode result)
        {
            result = null;

            if (!_states.TryGetValue(rootID, out var rootTree)) return false;

            if (!rootTree.TryGetAllStatesInChildren(out var all, true)) return false;

            result = all.FirstOrDefault(s => s.StateID == targetStateID);

            return result != null;
        }

        public bool TryGetAllSubStates(string rootID, out List<IStateNode> result)
        {
            result = null;

            if (!_states.TryGetValue(rootID, out var rootTree)) return false;

            return rootTree.TryGetAllStatesInChildren(out result);
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < _activeBranch.Count; i++)
            {
                _activeBranch[i].Update(deltaTime);
            }

            var appliedAny = false;

            foreach (var root in _states.Values)
            {
                var rootId = root.StateID; 

                if (_pendingPerRoot.TryGetValue(rootId, out var pending)) 
                {
                    var fromState = pending.From;
                    if (fromState is IStateNode ft && ft.GetRootState() != root) 
                    {
                        _pendingPerRoot.Remove(rootId); 
                    }
                    else if (fromState.IsExitReady) 
                    {
                        ApplyTransitionPath(pending.From, pending.To); 
                        if (pending.OneShot) _transitions.Remove(pending); 
                        _pendingPerRoot.Remove(rootId); 
                        appliedAny = true; 
                        continue; 
                    }
                    else
                    {
                        continue; 
                    }
                }

                ITransition selected = null;
                var bestPriority = int.MaxValue;

                for (int i = 0; i < _transitions.Count; i++)
                {
                    var t = _transitions[i];
                    
                    if (!RootsMatch(t.From, t.To)) continue;
                    
                    if (t.From is not IStateNode tf || tf.GetRootState() != root) continue;
                    
                    if (!IsTransitionSourceActive(t.From)) continue;
                    
                    if (!t.RequestTransition()) continue;
                    
                    var prio = t.Priority;
                    if (prio < bestPriority)
                    {
                        bestPriority = prio;
                        selected = t;
                    }
                }

                if (selected != null)
                {
                    if (selected.From.NeedsExitTime && !selected.From.IsExitReady) 
                    {
                        _pendingPerRoot[rootId] = selected; 
                        selected.From.RequestExit(); 
                        continue; 
                    }

                    ApplyTransitionPath(selected.From, selected.To);

                    if (selected.OneShot) _transitions.Remove(selected);

                    appliedAny = true;
                }
            }

            if (appliedAny)
            {
                RebuildActiveBranchFromActiveLeaf();
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            for (int i = 0; i < _activeBranch.Count; i++)
            {
                _activeBranch[i].FixedUpdate(deltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < _activeBranch.Count; i++)
            {
                _activeBranch[i].LateUpdate(deltaTime);
            }
        }

        private bool TryFindStateGlobal(string stateId, out IStateNode result)
        {
            result = null;

            foreach (var root in _states.Values)
            {
                if (root.StateID == stateId)
                {
                    result = root;
                    return true;
                }

                if (root.TryGetAllStatesInChildren(out var all, true))
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        if (all[i].StateID == stateId)
                        {
                            result = all[i];
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TryResolveRoot(IStateNode root, out string key, out IStateNode stored)
        {
            key = null;
            stored = null;
            if (root == null) return false;

            foreach (var kv in _states)
            {
                if (ReferenceEquals(kv.Value, root) || (!string.IsNullOrEmpty(root.StateID) && kv.Value.StateID == root.StateID))
                {
                    key = kv.Key;
                    stored = kv.Value;
                    return true;
                }
            }

            return false;
        }

        private static bool RootsMatch(IState a, IState b)
        {
            if (a is IStateNode ta && b is IStateNode tb)
            {
                return ta.GetRootState() == tb.GetRootState();
            }

            return true;
        }

        private bool IsTransitionSourceActive(IState from)
        {
            if (from is IStateNode ft)
            {
                for (int i = 0; i < _activeBranch.Count; i++)
                {
                    if (_activeBranch[i] == ft) return true;
                }

                return ft.IsActive;
            }

            return from.IsActive;
        }

        private void ApplyTransitionPath(IState from, IState to)
        {
            if (from == to) return;

            if (from is IStateNode f && to is IStateNode t)
            {
                var lca = FindLca(f, t);

                var cur = f;

                while (cur != null && cur != lca)
                {
                    cur.Exit();
                    cur = cur.GetParentState();
                }

                var stack = new Stack<IStateNode>();

                var down = t;

                while (down != null && down != lca)
                {
                    stack.Push(down);
                    down = down.GetParentState();
                }

                while (stack.Count > 0)
                {
                    stack.Pop().Enter();
                }

                CurrentStateID = t.GetRootState().StateID;
                return;
            }

            from.Exit();
            to.Enter();
            CurrentStateID = to.StateID;
        }

        private static IStateNode FindLca(IStateNode a, IStateNode b)
        {
            var visited = new HashSet<IStateNode>();

            var x = a;

            while (x != null)
            {
                visited.Add(x);
                x = x.GetParentState();
            }

            var y = b;

            while (y != null)
            {
                if (visited.Contains(y))
                {
                    return y;
                }

                y = y.GetParentState();
            }

            return null;
        }

        private void RebuildActiveBranchFromActiveLeaf()
        {
            _activeBranch.Clear();

            IStateNode leaf = null;

            foreach (var root in _states.Values)
            {
                if (!root.IsActive) continue;

                var q = new Queue<IStateNode>();

                q.Enqueue(root);

                while (q.Count > 0)
                {
                    var n = q.Dequeue();

                    if (n.IsActive)
                    {
                        leaf = n;
                    }

                    if (n is StateNode tt && tt.TryGetStatesInChildren(out var children))
                    {
                        for (int i = 0; i < children.Count; i++)
                        {
                            q.Enqueue(children[i]);
                        }
                    }
                }
            }

            while (leaf != null)
            {
                _activeBranch.Add(leaf);

                leaf = leaf.GetParentState();
            }

            _activeBranch.Reverse();

            if (_activeBranch.Count > 0)
            {
                CurrentStateID = _activeBranch[0].GetRootState().StateID;
            }
        }
    }
}