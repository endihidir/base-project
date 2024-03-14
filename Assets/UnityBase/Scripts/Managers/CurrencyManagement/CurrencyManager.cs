using System;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyDataService, ICurrencyViewService, IAppPresenterDataService
    {
        private const string COIN_AMOUNT_KEY = "CoinAmountKey";
        public event Action<int> OnCoinDataUpdate;

        private ICoinView _coinView;

        private int _startCoinAmount;
        
        private bool _isCoinSaveAvailable;
        
        public Transform CoinIconTransform => _coinView.CoinIconT;
        
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
        public void Start() { }
        public void Dispose() { }
        public void SetCoinViewData(ICoinView coinView)
        {
            _coinView = coinView;
        }

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
            _coinView.UpdateView(value);
        }
    }
}