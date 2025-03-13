
namespace UnityBase.StateMachineCore
{
    public abstract class StateMachine : IStateMachine<StateMachine>
    {
        private IState<StateMachine> _currentState;

        public void OnUpdate(float deltaTime)
        {
            _currentState.OnUpdate(deltaTime);
        }

        IState<StateMachine> IStateMachine<StateMachine>.CurrentState
        {
            get => _currentState;
            set => _currentState = value;
        }

        public bool TrySetState(IState<StateMachine> state)
        {
            if (state == _currentState) return false;

            _currentState?.OnExit();

            _currentState = state;
            _currentState.SetStateMachine(this);
            _currentState.OnEnter();

            return true;
        }
    }
}