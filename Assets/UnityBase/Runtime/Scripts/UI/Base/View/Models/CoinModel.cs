using System;
using TMPro;
using UnityBase.Observable;
using UnityBase.Service;

namespace UnityBase.UI.ViewCore
{
    public class CoinModel : ICoinModel
    {
        public Observable<int> Coins { get; }

        private readonly IJsonDataManager _jsonDataManager;

        private int _value;

        private TextMeshProUGUI _coinTxt;
        
        public CoinModel(IJsonDataManager jsonDataManager)
        {
            _jsonDataManager = jsonDataManager;

            _value = Deserialize().coins;
           
            Coins = new Observable<int>(_value, UpdateCoinView);
        }

        public ICoinModel Initialize(TextMeshProUGUI coinTxt)
        {
            _coinTxt = coinTxt;
            
            Coins?.Invoke();
            
            return this;
        }

        private void UpdateCoinView(int value)
        {
            _coinTxt.text = value.ToString("0");
            
            Serialize(new CoinData{coins = Coins.Value});
        }
        
        public CoinData Deserialize()
        {
            return _jsonDataManager.Load<CoinData>("CoinData");
        }

        public void Serialize(CoinData savedData)
        {
            _jsonDataManager.Save("CoinData", savedData);
        }

        public void Add(int value)
        {
            Coins.Set(Coins.Value + value);
        }

        public void Dispose()
        {
            Coins?.Dispose();
        }
    }
    
    public interface ICoinModel : IViewModel
    {
        public Observable<int> Coins { get; }
        public ICoinModel Initialize(TextMeshProUGUI coinTxt);
        CoinData Deserialize();
        void Serialize(CoinData savedData);
        public void Add(int value);
    }

    [Serializable]
    public struct CoinData
    {
        public int coins;
    }
}