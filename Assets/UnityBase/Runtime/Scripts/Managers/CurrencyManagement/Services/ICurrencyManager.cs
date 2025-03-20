using System;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICurrencyManager
    {
        public Transform CoinIconT { get; }
        public void SaveCoinData(int value);
        public void UpdateCoinView();
    }
}