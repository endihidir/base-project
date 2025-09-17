using System;
using UnityBase.Observable;
using UnityBase.SaveSystem;

namespace UnityBase.Runtime.Behaviours
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

        public void Add(int value) => Coins.Set(Coins.Value + value);
        
        public void Save()
        {
            Serialize(new CoinData { coins = Coins.Value });
        }

        public void Dispose()
        {
            Coins?.Dispose();
        }
    }
    
    public interface ICoinModel : IModel, ISaveData
    {
        public Observable<int> Coins { get; }
        public ICoinModel Initialize();
        public CoinData Deserialize();
        public void Serialize(CoinData savedData);
        public void Add(int value);
    }

    [Serializable]
    public struct CoinData
    {
        public int coins;
    }
}