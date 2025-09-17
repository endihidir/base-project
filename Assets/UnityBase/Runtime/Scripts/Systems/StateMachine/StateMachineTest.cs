using NaughtyAttributes;
using UnityBase.StateMachineCore;
using UnityEngine;

namespace UnityBase.Tag
{
    public class StateMachineTest : MonoBehaviour
    {
        private ITreeState _test1 = new TreeState();
        
        private ITreeState _subTest1 = new TreeState();
        private ITreeState _subTest2 = new TreeState(); 
        
        private ITreeState _subTest1_2 = new TreeState();
        private ITreeState _subTest2_2 = new TreeState(); 
        
        private ITreeState _subTest1_3 = new TreeState();
        private ITreeState _subTest2_3 = new TreeState();

        private ITransition _transition;

        private void Awake()
        {
            //_transition = new Transition(new TreeState("A"), new TreeState("B"), OnSuccess);
            
           // Test1();
        }

        private bool OnSuccess()
        {
            return Input.GetMouseButtonDown(1);
        }
        
        [Button]
        private void Test1()
        {
            _test1.Init(showLogs: false);
            
            _test1.AddSubState(_subTest1)?.AddSubState(_subTest1_2).AddSubState(_subTest1_3);
            _test1.AddSubState(_subTest2)?.AddSubState(_subTest2_2).AddSubState(_subTest2_3);
            
            _subTest1_2.Enter();
            _subTest2_2.Enter();
            
            Log();
            
            _subTest2.Exit();
            
            DebugLogger.LogError("-----------------------------------------");
            
            Log();
        }

        private void Log()
        {
            DebugLogger.LogError($"{_test1.StateID} : {_test1.IsActive}");

            if (_subTest1.TryGetAllStatesInChildren(out var subStateList1, true))
            {
                foreach (var state in subStateList1)
                {
                    DebugLogger.LogError($"{state.StateID} : {state.IsActive}");
                }
            }
            
            if (_subTest2.TryGetAllStatesInChildren(out var subStateList2, true))
            {
                foreach (var state in subStateList2)
                {
                    DebugLogger.LogError($"{state.StateID} : {state.IsActive}");
                }
            }
        }
    }
}