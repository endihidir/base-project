using System;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICoinManager
    {
        public Transform CoinIconT { get; }
        public void SaveData(int coin);
        public void UpdateView();
    }
}