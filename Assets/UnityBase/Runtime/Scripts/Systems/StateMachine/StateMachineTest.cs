using NaughtyAttributes;
using UnityBase.StateMachineCore;
using UnityEngine;
using VContainer;

namespace UnityBase.Tag
{
    public class HierarchicalStateMachineTest : MonoBehaviour
    {
        private IStateNode _test1 = new StateNode();
        
        private IStateNode _subTest1 = new StateNode();
        private IStateNode _subTest2 = new StateNode(); 
        
        private IStateNode _subTest1_2 = new StateNode();
        private IStateNode _subTest2_2 = new StateNode(); 
        
        private IStateNode _subTest1_3 = new StateNode();
        private IStateNode _subTest2_3 = new StateNode();

        private ITransition _transition;

        [Inject] 
        private readonly IStateMachineManager _stateMachineManager;

        private bool _cond;

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
        private void CreateInDependentStates()
        {
            _test1.Init(showLogs: false);
            
            _test1.AddSubState(_subTest1)?.AddSubState(_subTest1_2).AddSubState(_subTest1_3);
            _test1.AddSubState(_subTest2)?.AddSubState(_subTest2_2).AddSubState(_subTest2_3);
            
            _subTest1_2.Enter();
            _subTest2_2.Enter();
        }
        
        [Button]
        private void Log1()
        {
            Log(_test1);
            
            _subTest2.Exit();
            
            DebugLogger.LogError("-----------------------------------------");
            
            Log(_test1);
        }
        
        [Button]
        private void CreateDependentStates()
        {
            var sm = _stateMachineManager.GetOrRegister("PlayerStateMachine");
            _test1 = sm.CreateState(showLogs: false);
            
            _test1.AddSubState(_subTest1)?.AddSubState(_subTest1_2).AddSubState(_subTest1_3);
            _test1.AddSubState(_subTest2)?.AddSubState(_subTest2_2).AddSubState(_subTest2_3);
            
            sm.SetInitialState(_subTest1_2);
            sm.AddTransition(_subTest1_2, _subTest2_2, ()=> _cond);
        }
        
        [Button]
        private void Log2()
        {
            Log(_test1);
        }
        

        private void Log(IStateNode rootStateNode)
        {
            DebugLogger.LogError($"{rootStateNode.StateID} : {rootStateNode.IsActive}");
            
            DebugLogger.LogError("*********");

            if (_subTest1.TryGetAllStatesInChildren(out var subStateList1, true))
            {
                foreach (var state in subStateList1)
                {
                    DebugLogger.LogError($"{state.StateID} : {state.IsActive}");
                }
            }
            
            DebugLogger.LogError("*********");
            
            if (_subTest2.TryGetAllStatesInChildren(out var subStateList2, true))
            {
                foreach (var state in subStateList2)
                {
                    DebugLogger.LogError($"{state.StateID} : {state.IsActive}");
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _cond = !_cond;
            }
        }
    }
}