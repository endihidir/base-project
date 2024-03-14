using UnityEngine;

namespace UnityBase.Manager
{
    public interface ICoinView
    {
        public Transform CoinIconT { get; }
        public void UpdateView(int val);
    }
}