namespace UnityBase.Service
{
    public interface IGameDataService
    {
        public void Initialize();
        public void Start();
        public void PlayGame();
        public void PauseGame();
        public void Dispose();
    }
}