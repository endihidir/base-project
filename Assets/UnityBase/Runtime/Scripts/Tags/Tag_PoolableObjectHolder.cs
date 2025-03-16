using NaughtyAttributes;
using UnityBase.StateMachineCore;
using UnityEngine;

namespace UnityBase.Tag
{
    public class Tag_PoolableObjectHolder : MonoBehaviour
    {
        private IState _test1 = new StateBase<string>("Test1", true, false);
        
        private IState _subTest1 = new StateBase<string>("SubTest1", false, false);
        private IState _subTest2 = new StateBase<string>("SubState2", false, false); 
        
        private IState _newSubTest1 = new StateBase<string>("NewSubTest1", false, false);
        private IState _newSubTest2 = new StateBase<string>("NewSubTest2", false, false);
        
        [Button]
        private void Test()
        {
            _test1.AddSubState(_subTest1)?.AddSubState(_newSubTest1);
            _test1.AddSubState(_subTest2)?.AddSubState(_newSubTest2);
            
            _test1.Enter();
            
            if (_subTest1.GetRootState().TryGetAllSubStates(out var subStateList))
            {
                foreach (var state in subStateList)
                {
                    Debug.LogError(state.StateName);
                }
            }
        }
    }
}