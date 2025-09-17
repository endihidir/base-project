using System.Collections.Generic;

namespace UnityBase.BlackboardCore
{
    public interface IBlackboardRegistry
    {
        IBlackboard GetOrCreate(int id);
        bool TryGet(int id, out IBlackboard blackboard);
        bool Remove(int id);
        void Clear();
        int Count { get; }
    }
    
    public class BlackboardRegistry : IBlackboardRegistry
    {
        private readonly Dictionary<int, IBlackboard> _map = new();

        public IBlackboard GetOrCreate(int id)
        {
            if (_map.TryGetValue(id, out var bb)) return bb;
            bb = new Blackboard();
            _map[id] = bb;
            return bb;
        }

        public bool TryGet(int id, out IBlackboard blackboard) => _map.TryGetValue(id, out blackboard);
        public bool Remove(int id) => _map.Remove(id);

        public void Clear() => _map.Clear();
        public int Count => _map.Count;
    }
}