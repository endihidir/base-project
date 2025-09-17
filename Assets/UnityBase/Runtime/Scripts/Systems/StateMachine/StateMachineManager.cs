using System.Collections.Generic;
using UnityBase.BlackboardCore;
using UnityEngine;
using VContainer.Unity;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachineManager
    {
        IStateMachine GetOrRegister(string id, IBlackboard blackboard = null, bool showLogs = true);
        bool TryGet(string id, out IStateMachine sm);
        void Set(string id, IStateMachine sm);
        bool Remove(string id);
        void Clear();
        int Count();
        IReadOnlyCollection<IStateMachine> All { get; }
    }
    
    public class StateMachineManager : IStateMachineManager, ITickable, IFixedTickable, ILateTickable
    {
        private readonly Dictionary<string, IStateMachine> _map = new();

        public IStateMachine GetOrRegister(string id, IBlackboard blackboard = null, bool showLogs = true)
        {
            if (_map.TryGetValue(id, out var sm)) return sm;
            sm = new StateMachine();
            _map[id] = sm;
            return sm;
        }

        public bool TryGet(string id, out IStateMachine sm) => _map.TryGetValue(id, out sm);
        public void Set(string id, IStateMachine sm) => _map[id] = sm;
        public bool Remove(string id) => _map.Remove(id);
        public void Clear() => _map.Clear();
        public int Count() => _map.Count;
        public IReadOnlyCollection<IStateMachine> All => _map.Values;
        
        public void Tick()
        {
            foreach (var keyValuePair in _map)
            {
                keyValuePair.Value.Update(Time.deltaTime);
            }
        }

        public void FixedTick()
        {
            foreach (var keyValuePair in _map)
            {
                keyValuePair.Value.FixedUpdate(Time.fixedDeltaTime);
            }
        }

        public void LateTick()
        {
            foreach (var keyValuePair in _map)
            {
                keyValuePair.Value.LateUpdate(Time.deltaTime);
            }
        }
    }
}