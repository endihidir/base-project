using UnityBase.BootService;
using UnityBase.Service;
using UnityBase.UI.Dynamic;
using UnityBase.UI.ViewCore;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManager, IAppBootService
    {
        private readonly IViewBehaviourFactory _viewBehaviourFactory;
        public CurrencyManager(IViewBehaviourFactory viewBehaviourFactory)
        {
            _viewBehaviourFactory = viewBehaviourFactory;
        }
        
        public void Initialize() { }

        public Transform CoinIconT
        {
            get
            {
                _viewBehaviourFactory.TryGetView<CoinViewUI, CoinView>(out var coinView);
                return coinView.CoinIconTransform; 
            }
        }

        public void SaveCoinData(int value)
        {
            if (_viewBehaviourFactory.TryGetModel<CoinViewUI, CoinModel>(out var coinViewModel))
            {
                coinViewModel.Add(value);
            }
            else
            {
                Debug.LogError("Coin model does not exist!");
            }
        }

        public void UpdateCoinView()
        {
            if (_viewBehaviourFactory.TryGetView<CoinViewUI, CoinView>(out var coinViewModel))
            {
               coinViewModel.UpdateView();
            }
            else
            {
                Debug.LogError("Coin model does not exist!");
            }
        }

        public void Dispose() { }
    }
}