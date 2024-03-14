using UnityBase.Manager;

namespace UnityBase.Service
{
    public interface IGameplayDataService
    {
        public GameState CurrentGameState { get; }
        public void ChangeGameState(GameState gameState, float transitionDuration, float startDelay = 0f);
    }
}