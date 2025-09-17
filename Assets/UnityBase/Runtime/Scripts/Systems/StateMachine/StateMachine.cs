using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.BlackboardCore;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachine
    {
        public string CurrentStateID { get; }
        public IReadOnlyList<ITreeState> ActiveBranch { get; }

        public bool TryGetRootState(string id, out ITreeState treeState);

        public ITreeState CreateState(string stateID = null);
        public ITreeState RegisterRoot(ITreeState root);
        public ITreeState EnsureSubStateChain(string rootID, params string[] chain);
        public ITreeState EnsureSubStateChain(ITreeState root, params string[] chain);

        public bool TryFindState(string rootID, string stateID, out ITreeState result);
        public bool TryFindStateByPath(string path, out ITreeState result);

        public IStateMachine SetInitialState(string targetStateID);
        public IStateMachine SetInitialState(string rootID, string targetStateID);
        public IStateMachine SetInitialState(ITreeState target);

        public IStateMachine AddTransition(string from, string to, Func<bool> condition);
        public IStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition);
        public IStateMachine AddTransition(ITreeState from, ITreeState to, Func<bool> condition);

        public IStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot); 
        public IStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition, int priority, bool oneShot); 
        public IStateMachine AddTransition(ITreeState from, ITreeState to, Func<bool> condition, int priority, bool oneShot); 

        public bool TryGetSubState(string rootID, string targetStateID, out ITreeState result);
        public bool TryGetAllSubStates(string rootID, out List<ITreeState> result);

        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
    }

    public class StateMachine : IStateMachine
    {
        private readonly Dictionary<string, ITreeState> _states = new();
        private readonly List<ITransition> _transitions = new();
        private readonly List<ITreeState> _activeBranch = new();
        private readonly Dictionary<string, ITransition> _pendingPerRoot = new(); 

        private readonly IBlackboard _defaultBlackboard;
        private readonly bool _defaultShowLogs;

        public IReadOnlyList<ITreeState> ActiveBranch => _activeBranch;
        public string CurrentStateID { get; private set; }

        public StateMachine(IBlackboard defaultBlackboard = null, bool showLogs = true)
        {
            _defaultBlackboard = defaultBlackboard;
            _defaultShowLogs = showLogs;
        }

        public bool TryGetRootState(string id, out ITreeState treeState)
        {
            return _states.TryGetValue(id, out treeState);
        }

        public ITreeState CreateState(string stateID = null)
        {
            if (string.IsNullOrEmpty(stateID))
            {
                var root = new TreeState();
                root.Init(_defaultBlackboard, _defaultShowLogs);
                if (_states.TryGetValue(root.StateID, out var existing)) return existing;
                _states[root.StateID] = root;
                return root;
            }
            else
            {
                if (_states.TryGetValue(stateID, out var existing)) return existing;

                var root = new TreeState(stateID);
                root.Init(_defaultBlackboard, _defaultShowLogs);
                _states[stateID] = root;
                return root;
            }
        }

        public ITreeState RegisterRoot(ITreeState root)
        {
            if (root == null || string.IsNullOrEmpty(root.StateID)) return null;

            if (_states.TryGetValue(root.StateID, out var existing))
            {
                return existing;
            }

            root.Init(_defaultBlackboard, _defaultShowLogs);
            _states[root.StateID] = root;
            return root;
        }

        public ITreeState EnsureSubStateChain(string rootID, params string[] chain)
        {
            if (chain == null || chain.Length == 0) return null;
            if (!_states.TryGetValue(rootID, out var root)) return null;

            var current = root;

            for (int i = 0; i < chain.Length; i++)
            {
                var id = chain[i];

                if (!current.TryGetStateInChildren(id, out var found))
                {
                    found = new TreeState(id);
                    current.AddSubState(found);
                }

                current = found;
            }

            return current;
        }

        public ITreeState EnsureSubStateChain(ITreeState root, params string[] chain)
        {
            if (root == null || chain == null || chain.Length == 0) return null;
            if (!TryResolveRoot(root, out _, out var storedRoot)) return null;

            var current = storedRoot;

            for (int i = 0; i < chain.Length; i++)
            {
                var id = chain[i];

                if (!current.TryGetStateInChildren(id, out var found))
                {
                    found = new TreeState(id);
                    current.AddSubState(found);
                }

                current = found;
            }

            return current;
        }

        public bool TryFindState(string rootID, string stateID, out ITreeState result)
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

        public bool TryFindStateByPath(string path, out ITreeState result)
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

        public IStateMachine SetInitialState(string targetStateID)
        {
            if (!TryFindStateGlobal(targetStateID, out var target))
            {
                DebugLogger.LogError($"Initial state with ID '{targetStateID}' not found in any root state.");
                return null;
            }

            return SetInitialState(target);
        }

        public IStateMachine SetInitialState(string rootID, string targetStateID)
        {
            if (!TryFindState(rootID, targetStateID, out var target))
            {
                DebugLogger.LogError($"Initial state with ID '{targetStateID}' not found under root '{rootID}'.");
                return null;
            }

            return SetInitialState(target);
        }

        public IStateMachine SetInitialState(ITreeState target)
        {
            if (target == null) return null;

            var root = target.GetRootState();

            if (!root.IsActive) root.Enter();

            var stack = new Stack<ITreeState>();

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

        public IStateMachine AddTransition(string fromId, string toId, Func<bool> condition)
        {
            if (!TryFindStateGlobal(fromId, out var from) || !TryFindStateGlobal(toId, out var to))
            {
                DebugLogger.LogError($"Cannot create transition from '{fromId}' to '{toId}'. States not found.");
                return null;
            }

            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IStateMachine AddTransition(string rootID, string fromId, string toId, Func<bool> condition)
        {
            if (!TryFindState(rootID, fromId, out var from) || !TryFindState(rootID, toId, out var to))
            {
                DebugLogger.LogError($"Cannot create transition from '{fromId}' to '{toId}' under root '{rootID}'. States not found.");
                return null;
            }

            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IStateMachine AddTransition(ITreeState from, ITreeState to, Func<bool> condition)
        {
            if (from == null || to == null) return this;
            _transitions.Add(new Transition(from, to, condition));
            return this;
        }

        public IStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (!TryFindStateGlobal(from, out var f) || !TryFindStateGlobal(to, out var t))
            {
                DebugLogger.LogError($"Cannot create transition from '{from}' to '{to}'. States not found.");
                return null;
            }
            _transitions.Add(new Transition(f, t, condition, priority, oneShot)); 
            return this; 
        }

        public IStateMachine AddTransition(string rootID, string from, string to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (!TryFindState(rootID, from, out var f) || !TryFindState(rootID, to, out var t))
            {
                DebugLogger.LogError($"Cannot create transition from '{from}' to '{to}' under root '{rootID}'. States not found.");
                return null;
            }
            _transitions.Add(new Transition(f, t, condition, priority, oneShot)); 
            return this; 
        }

        public IStateMachine AddTransition(ITreeState from, ITreeState to, Func<bool> condition, int priority, bool oneShot) 
        {
            if (from == null || to == null) return this; 
            _transitions.Add(new Transition(from, to, condition, priority, oneShot)); 
            return this; 
        }

        public bool TryGetSubState(string rootID, string targetStateID, out ITreeState result)
        {
            result = null;

            if (!_states.TryGetValue(rootID, out var rootTree)) return false;

            if (!rootTree.TryGetAllStatesInChildren(out var all, true)) return false;

            result = all.FirstOrDefault(s => s.StateID == targetStateID);

            return result != null;
        }

        public bool TryGetAllSubStates(string rootID, out List<ITreeState> result)
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
                    if (fromState is ITreeState ft && ft.GetRootState() != root) 
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

                    if (t.From is not ITreeState tf || tf.GetRootState() != root) continue;

                    if (!IsTransitionSourceActive(t.From)) continue;

                    if (!t.TryInvokeTransition()) continue;

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

        private bool TryFindStateGlobal(string stateId, out ITreeState result)
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

        private bool TryResolveRoot(ITreeState root, out string key, out ITreeState stored)
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
            if (a is ITreeState ta && b is ITreeState tb)
            {
                return ta.GetRootState() == tb.GetRootState();
            }

            return true;
        }

        private bool IsTransitionSourceActive(IState from)
        {
            if (from is ITreeState ft)
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

            if (from is ITreeState f && to is ITreeState t)
            {
                var lca = FindLca(f, t);

                var cur = f;

                while (cur != null && cur != lca)
                {
                    cur.Exit();
                    cur = cur.GetParentState();
                }

                var stack = new Stack<ITreeState>();

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

        private static ITreeState FindLca(ITreeState a, ITreeState b)
        {
            var visited = new HashSet<ITreeState>();

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

            ITreeState leaf = null;

            foreach (var root in _states.Values)
            {
                if (!root.IsActive) continue;

                var q = new Queue<ITreeState>();

                q.Enqueue(root);

                while (q.Count > 0)
                {
                    var n = q.Dequeue();

                    if (n.IsActive)
                    {
                        leaf = n;
                    }

                    if (n is TreeState tt && tt.TryGetStatesInChildren(out var children))
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