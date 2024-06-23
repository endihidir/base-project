using System;
using UnityBase.Service;
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
                _viewBehaviourFactory.TryGetViewAnimation<CoinUI, BounceView>(out var coinViewAnim);
                return coinViewAnim.CoinIconTransform; 
            }
        }

        public void Collect(int coins)
        {
            if (_viewBehaviourFactory.TryGetModel<CoinUI, CoinModel>(out var coinViewModel))
            {
                coinViewModel.Add(coins);
            }
            else
            {
                Debug.LogError("Coin model does not exist!");
            }
        }

        public void PlayBounceAnim(Action onComplete)
        {
            if (_viewBehaviourFactory.TryGetViewAnimation<CoinUI, BounceView>(out var coinViewAnim))
            {
                coinViewAnim.Bounce(onComplete);
            }
            else
            {
                Debug.LogError("Coin bounce animation does not exist!");
            }
        }

        public void Dispose() { }
    }
}