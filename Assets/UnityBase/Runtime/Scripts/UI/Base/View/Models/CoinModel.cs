using System;
using UnityBase.Observable;
using UnityBase.Service;

namespace UnityBase.UI.ViewCore
{
    public class CoinModel : ICoinModel
    {
        public Observable<int> Coins { get; }

        private readonly ISaveManager _saveManager;
        
        public CoinModel(ISaveManager saveManager)
        {
            _saveManager = saveManager;
           
            Coins = new Observable<int>(Deserialize().coins);
        }

        public ICoinModel Initialize()
        {
            Coins?.Invoke();
            
            return this;
        }
        
        public CoinData Deserialize()
        {
            return _saveManager.LoadFromPrefs<CoinData>("CoinData");
        }

        public void Serialize(CoinData savedData)
        {
            _saveManager.SaveToPrefs("CoinData", savedData);
        }

        public void Add(int value)
        {
            Coins.Set(Coins.Value + value);
            
            Serialize(new CoinData{coins = Coins.Value});
        }
        
        public void Dispose()
        {
            Coins?.Dispose();
        }
    }
    
    public interface ICoinModel : IModel
    {
        public Observable<int> Coins { get; }
        public ICoinModel Initialize();
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