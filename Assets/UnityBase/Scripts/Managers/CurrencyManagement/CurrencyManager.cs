using System;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManager, ICurrencyViewService, IAppBootService
    {
        private const string COIN_AMOUNT_KEY = "CoinAmountKey";
        public event Action<int> OnCoinDataUpdate;

        private IViewUI _viewUI;

        private int _startCoinAmount;
        
        private bool _isCoinSaveAvailable;
        
        public Transform CoinIconTransform => null;
        
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
        public void SetCoinViewData(IViewUI viewUI) => _viewUI = viewUI;
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
        
        public void UpdateCoinView(int value)
        {
            // _viewUI.UpdateView(value);
        }
    }
}