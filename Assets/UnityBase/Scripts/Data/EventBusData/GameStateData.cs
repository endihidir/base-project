using UnityBase.EventBus;

namespace UnityBase.Manager.Data
{
    public struct GameStateData : IEvent
    {
        public static int GetChannel(TransitionState transitionState) => (int)transitionState;
        public GameState StartState { get; set; }
        public GameState EndState { get; set; }
        public float Duration{ get; set; }
        
        public struct Builder
        {
            private GameState _startState;
            private GameState _endState;
            private float _duration;
            public Builder WithStartState(GameState startState)
            {
                _startState = startState;
                return this;
            }

            public Builder WithEndState(GameState endState)
            {
                _endState = endState;
                return this;
            }

            public Builder WithDuration(float duration)
            {
                _duration = duration;
                return this;
            }

            public GameStateData Build()
            {
                var gameStateData = new GameStateData()
                {
                    StartState = _startState,
                    EndState = _endState,
                    Duration = _duration
                };

                return gameStateData;
            }
        }
    }
}

public enum TransitionState
{
    Start,
    Middle,
    End
}