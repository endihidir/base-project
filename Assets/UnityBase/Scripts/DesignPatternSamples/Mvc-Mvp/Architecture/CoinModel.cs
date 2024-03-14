using System;
using UnityBase.Observable;

namespace UnityBase.Mvc.Architecture
{
    public class CoinModel : ICoinModel
    {
        public Observable<int> Coins { get; }
        
        public CoinData Serialize()
        {
            return default;
        }

        public void Deserialize(CoinData savedData)
        {
            
        }
    }

    public interface ICoinModel
    {
        Observable<int> Coins { get; }
        
        CoinData Serialize();
        
        void Deserialize(CoinData savedData);
    }

    [Serializable]
    public struct CoinData
    {
        
    }
}