using UnityBase.Observable;
using UnityBase.Service;

namespace UnityBase.UI.ViewCore
{
    public class CoinViewModel : ICoinModel
    {
        public Observable<int> Coins { get; }

        private readonly IJsonDataManager _jsonDataManager;

        private int _value;
        
        public CoinViewModel(IJsonDataManager jsonDataManager)
        {
            _jsonDataManager = jsonDataManager;
           
            Coins = new Observable<int>(_value);
        }
        
        public CoinData Serialize()
        {
            return _jsonDataManager.Load<CoinData>("CoinData");
        }

        public void Deserialize(CoinData savedData)
        {
            _jsonDataManager.Save("CoinData", savedData);
        }
        
        public void Dispose() => Coins?.Dispose();
    }
    
    public interface ICoinModel : IViewModel
    {
        public Observable<int> Coins { get; }
        CoinData Serialize();
        void Deserialize(CoinData savedData);
    }

    public struct CoinData
    {
        
    }
}