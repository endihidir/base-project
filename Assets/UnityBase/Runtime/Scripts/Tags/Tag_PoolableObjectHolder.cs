using System;
using NaughtyAttributes;
using UnityBase.BlackboardCore;
using UnityBase.StateMachineCore;
using UnityEngine;
using VContainer;

namespace UnityBase.Tag
{
    public class Tag_PoolableObjectHolder : MonoBehaviour
    {
        private ITreeState _test1 = new TreeState("Test1");
        
        private ITreeState _subTest1 = new TreeState("SubTest1");
        private ITreeState _subTest2 = new TreeState("SubState2"); 
        
        private ITreeState _newSubTest1 = new TreeState("NewSubTest1");
        private ITreeState _newSubTest2 = new TreeState("NewSubTest2");

        private void Awake()
        {
            _test1.Init().Enter();
            
            Test1();
        }

        /*[Inject]
        private void Init(IBlackboard blackboard)
        {
            _test1.InitWith(blackboard).Enter();
            
            Test1();
        }*/
        
        [Button]
        private void Test1()
        {
            _test1.AddSubState(_subTest1)?.AddSubState(_newSubTest1);
            _test1.AddSubState(_subTest2)?.AddSubState(_newSubTest2);
            
            Debug.LogError(_subTest1.GetRootState().StateID);
            
            if (_subTest1.GetRootState().TryGetAllSubStates(out var subStateList))
            {
                foreach (var state in subStateList)
                {
                    Debug.LogError(state.StateID);
                }
            }
        }
    }
}