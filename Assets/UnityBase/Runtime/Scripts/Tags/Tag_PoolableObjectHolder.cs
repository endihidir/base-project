using NaughtyAttributes;
using UnityBase.StateMachineCore;
using UnityEngine;

namespace UnityBase.Tag
{
    public class Tag_PoolableObjectHolder : MonoBehaviour
    {
        private ITreeState _test1 = new TreeState("Test1");
        
        private ITreeState _subTest1 = new TreeState("SubTest1");
        private ITreeState _subTest2 = new TreeState("SubState2"); 
        
        private ITreeState _newSubTest1 = new TreeState("NewSubTest1");
        private ITreeState _newSubTest2 = new TreeState("NewSubTest2");

        private ITransition _transition;

        private void Awake()
        {
            //_transition = new Transition(new TreeState("A"), new TreeState("B"), OnSuccess);
            
            Test1();
        }

        private bool OnSuccess()
        {
            return Input.GetMouseButtonDown(1);
        }
        
        [Button]
        private void Test1()
        {
            _test1.Init();
            _test1.AddSubState(_subTest1)?.AddSubState(_newSubTest1);
            _test1.AddSubState(_subTest2)?.AddSubState(_newSubTest2);
            _newSubTest2.Enter();
            _newSubTest1.Enter();
            
            Log();
            
            _subTest2.Exit();
            
            Log();
        }

        private void Log()
        {
            Debug.LogError($"{_test1.StateID} : {_test1.IsActive}");

            if (_subTest1.GetRootState().TryGetAllStatesInChildren(out var subStateList))
            {
                foreach (var state in subStateList)
                {
                    Debug.LogError($"{state.StateID} : {state.IsActive}");
                }
            }
        }
    }
}