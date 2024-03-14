
namespace UnityBase.Mvc.Architecture
{
    public class CoinController : ICoinController
    {
        private readonly ICoinModel _coinModel;
        private readonly ICoinView _coinView;
        private readonly ICoinService _coinService;

        public CoinController(ICoinView coinView, ICoinService coinService)
        {
            Preconditions.CheckNotNull(coinView, "CoinView cannot be null");
            Preconditions.CheckNotNull(coinService, "CoinService cannot be null");
            
            _coinView = coinView;
            _coinService = coinService;

            _coinModel = Load();
            _coinModel.Coins.AddListener(UpdateView);
            _coinModel.Coins.Invoke();
        }

        public void Collect(int coins) => _coinModel.Coins.Set(_coinModel.Coins.Value + coins);
        public void UpdateView(int coins) => _coinView.UpdateCoinsDisplay(coins);

        public void Save() => _coinService.Save(_coinModel);
        public ICoinModel Load() => _coinService.Load();
        
        public struct Builder
        {
            private ICoinView _coinView;
            private ICoinService _coinService;

            public Builder WithService(ICoinService coinService)
            {
                _coinService = coinService;
                return this;
            }

            public Builder WithView(ICoinView coinView)
            {
                _coinView = coinView;
                return this;
            }

            public CoinController Build() => new(_coinView, _coinService);
        }
    }

    public interface ICoinController
    {
        void Collect(int coins);
        void UpdateView(int coins);
        void Save();
        ICoinModel Load();
    }
}