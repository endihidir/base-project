using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace UnityBase.StateMachineCore
{
    public interface IStateMachineManager
    {
        IHierarchicalStateMachine GetOrRegister(string id);
        bool TryGet(string id, out IHierarchicalStateMachine sm);
        void Set(string id, IHierarchicalStateMachine sm);
        bool Remove(string id);
        void Clear();
        int Count();
        IReadOnlyCollection<IHierarchicalStateMachine> All { get; }
        IStateMachine GetOrRegisterFsm<T>(string id) where T : StateBase;
        bool TryGetFsm<T>(string id, out IStateMachine sm) where T : StateBase; 
        void SetFsm<T>(string id, IStateMachine sm) where T : StateBase;
        bool RemoveFsm<T>(string id) where T : StateBase;    
        int CountFsm();                                                       
        IReadOnlyCollection<IStateMachineBase> AllFsm { get; }               
    }

    public class StateMachineManager : IStateMachineManager, ITickable, IFixedTickable, ILateTickable
    {
        private readonly Dictionary<string, IHierarchicalStateMachine> _map = new();
        
        private readonly Dictionary<(string id, Type t), IStateMachineBase> _fsmMap = new(); 

        public IHierarchicalStateMachine GetOrRegister(string id)
        {
            if (_map.TryGetValue(id, out var sm)) return sm;
            sm = new HierarchicalStateMachine();
            _map[id] = sm;
            return sm;
        }

        public bool TryGet(string id, out IHierarchicalStateMachine sm) => _map.TryGetValue(id, out sm);
        public void Set(string id, IHierarchicalStateMachine sm) => _map[id] = sm;
        public bool Remove(string id) => _map.Remove(id);

        public int Count() => _map.Count;
        public IReadOnlyCollection<IHierarchicalStateMachine> All => _map.Values;
        
        public IStateMachine GetOrRegisterFsm<T>(string id) where T : StateBase
        {
            var key = (id, typeof(T));
            
            if (_fsmMap.TryGetValue(key, out var baseSm))
                return (IStateMachine)baseSm;

            var sm = new StateMachine();
            _fsmMap[key] = sm;
            return sm;
        }

        public bool TryGetFsm<T>(string id, out IStateMachine sm) where T : StateBase
        {
            var ok = _fsmMap.TryGetValue((id, typeof(T)), out var baseSm);
            sm = ok ? (IStateMachine)baseSm : null;
            return ok;
        }

        public void SetFsm<T>(string id, IStateMachine sm) where T : StateBase
        {
            _fsmMap[(id, typeof(T))] = sm;
        }

        public bool RemoveFsm<T>(string id) where T : StateBase
        {
            return _fsmMap.Remove((id, typeof(T)));
        }

        public int CountFsm() => _fsmMap.Count;

        public IReadOnlyCollection<IStateMachineBase> AllFsm => _fsmMap.Values;

        public void Tick()
        {
            foreach (var kv in _map)
                kv.Value.Update(Time.deltaTime);

            foreach (var kv in _fsmMap)
                kv.Value.Update(Time.deltaTime);
        }

        public void FixedTick()
        {
            foreach (var kv in _map)
                kv.Value.FixedUpdate(Time.fixedDeltaTime);

            foreach (var kv in _fsmMap)
                kv.Value.FixedUpdate(Time.fixedDeltaTime);
        }

        public void LateTick()
        {
            foreach (var kv in _map)
                kv.Value.LateUpdate(Time.deltaTime);

            foreach (var kv in _fsmMap)
                kv.Value.LateUpdate(Time.deltaTime);
        }
        
        public void Clear()
        {
            _map.Clear();
            _fsmMap.Clear(); 
        }
    }
}
