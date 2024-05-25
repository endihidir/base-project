using System;

namespace UnityBase.Service
{
    public interface ICurrencyManagementService
    {
        public event Action<int> OnCoinDataUpdate;
        public int SavedCoinAmount { get; }
        public void IncreaseCoinData(int value);
        public void DecreaseCoinData(int value);
    }
}