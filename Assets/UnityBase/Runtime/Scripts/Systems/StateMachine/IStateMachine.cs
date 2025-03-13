
namespace UnityBase.StateMachineCore
{
    internal interface IStateMachine<T> where T : StateMachine
    {
        public IState<T> CurrentState { get; protected set; }
        public void OnUpdate(float deltaTime);
        public bool TrySetState(IState<T> state);
    }
}
