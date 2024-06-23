using System;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICoinManager
    {
        public Transform CoinIconT { get; }
        public void Collect(int coins);
        public void PlayBounceAnim(Action onComplete);
    }
}