using UnityBase.Manager;
using VContainer.Unity;

namespace UnityBase.Presenters
{
    public class GamePresenter : IInitializable
    {
        private readonly IGameManager _gameManager;
        
        public GamePresenter(IGameManager gameManager) => _gameManager = gameManager;

        public void Initialize() => _gameManager.Initialize();
    }
}
