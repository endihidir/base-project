using UnityBase.BootService;
using UnityBase.Service;
using UnityBase.UI.Dynamic;
using UnityBase.UI.ViewCore;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CoinManager : ICoinManager, IAppBootService
    {
        private readonly IViewBehaviourFactory _viewBehaviourFactory;
        public CoinManager(IViewBehaviourFactory viewBehaviourFactory)
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

        public void SaveData(int coin)
        {
            if (_viewBehaviourFactory.TryGetModel<CoinViewUI, CoinModel>(out var coinViewModel))
            {
                coinViewModel.Add(coin);
            }
            else
            {
                Debug.LogError("Coin model does not exist!");
            }
        }

        public void UpdateView()
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