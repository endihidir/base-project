using UnityBase.Manager;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenters
{
    public class GameplayPresenter : IInitializable
    {
        private readonly IGameplayManager _gameplayManager;
        
        public GameplayPresenter(IObjectResolver objectResolver, IGameplayManager gameManager, AmbientResolverProvider ambientResolverProvider)
        {
            _gameplayManager = gameManager;
            
            ambientResolverProvider.UpdateResolver(objectResolver);
        }

        public void Initialize() => _gameplayManager.Initialize();
    }
}