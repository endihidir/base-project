using UnityBase.Manager;
using VContainer.Unity;

namespace UnityBase.Presenters
{
    public class GameplayPresenter : IInitializable
    {
        private readonly IGameplayManager _gameplayManager;
        
        public GameplayPresenter(IGameplayManager gameManager) => _gameplayManager = gameManager;

        public void Initialize() => _gameplayManager.Initialize();
    }
}