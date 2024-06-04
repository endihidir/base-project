using System;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManagementService, ICurrencyViewService, IAppBootService
    {
        private const string COIN_AMOUNT_KEY = "CoinAmountKey";
        public event Action<int> OnCoinDataUpdate;

        private ICurrencyView _currencyView;

        private int _startCoinAmount;
        
        private bool _isCoinSaveAvailable;
        
        public Transform CoinIconTransform => _currencyView.CoinIconT;
        
        public int SavedCoinAmount
        {
            get => PlayerPrefs.GetInt(COIN_AMOUNT_KEY, _startCoinAmount);
            private set => PlayerPrefs.SetInt(COIN_AMOUNT_KEY, value);
        }

        public CurrencyManager(ManagerDataHolderSO managerDataHolderSo)
        {
            var currencyManagerData = managerDataHolderSo.currencyManagerSo;

            _startCoinAmount = currencyManagerData.startCoinAmount;
        }
        
        ~CurrencyManager() { }

        public void Initialize() { }
        public void Dispose() { }
        public void SetCoinViewData(ICurrencyView currencyView) => _currencyView = currencyView;
        public void IncreaseCoinData(int value)
        {
            SavedCoinAmount += value;

            OnCoinDataUpdate?.Invoke(SavedCoinAmount);
        }

        public void DecreaseCoinData(int value)
        {
            SavedCoinAmount -= value;

            OnCoinDataUpdate?.Invoke(SavedCoinAmount);
        }
        
        public void UpdateCoinView(int value) => _currencyView.UpdateView(value);
    }
}