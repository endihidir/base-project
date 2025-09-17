using System;
using UnityBase.Runtime.Behaviours;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICurrencyManager
    {
        void BindCoinContext(IOwnerContext context);
        void UnbindCoinContext(IOwnerContext context);
        void PlayCollect(int amount, CoinIconTest prefab, Transform parent, Vector3 screenPos);
    }
}