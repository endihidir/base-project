using UnityEngine;

namespace UnityBase.Service
{
    public interface ICurrencyViewService
    {
        public Transform CoinIconTransform { get; }
        public void UpdateCoinView(int value);
    }
}