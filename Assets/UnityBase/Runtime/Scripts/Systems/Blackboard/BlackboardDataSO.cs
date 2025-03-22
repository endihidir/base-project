using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.BlackboardCore
{
    [CreateAssetMenu(fileName = "New Blackboard Data", menuName = "Game/Blackboard/Blackboard Data")]
    public class BlackboardDataSO : ScriptableObject
    {
        public List<BlackboardEntryData> entries;

        public void SetValuesOnBlackboard(IBlackboard blackboard)
        {
            foreach (var entry in entries)
            {
                entry.SetValueOnBlackBoard(blackboard);
            }
        }
    }
    
    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver
    {
        public string keyName;
        
        public AnyValue.ValueType valueType;
        
        public AnyValue value;

        private static Dictionary<AnyValue.ValueType, Action<IBlackboard, BlackboardKey, AnyValue>> _setValueDispatchTable = new()
            {
                {AnyValue.ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue)},
                {AnyValue.ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue)},
                {AnyValue.ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue)},
                {AnyValue.ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<string>(key, anyValue)},
                {AnyValue.ValueType.Vector3, (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue)},
            };

        public void SetValueOnBlackBoard(IBlackboard blackboard)
        {
            var key = blackboard.GetOrRegisterKey(keyName);
            
            if (_setValueDispatchTable.TryGetValue(value.type, out var action))
            {
                action(blackboard, key, value);
            }
            else
            {
                Debug.LogError($"Unsupported value type: {value.type}");
            }
        }
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            value.type = valueType;
        }
    }
}