using System;

namespace UnityBase.StateMachineCore
{
    public interface IState<T> where T : StateMachine
    {
        public event Action OnStateComplete;
        public T StateMachine { get; protected set; }
        public void SetStateMachine(T stateMachine);
        public void OnEnter();
        public void OnUpdate(float deltaTime);
        public void OnExit();

        protected void SetStateComplete();
    }
}