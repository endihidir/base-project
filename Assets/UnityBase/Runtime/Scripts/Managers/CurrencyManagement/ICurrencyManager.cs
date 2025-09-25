using UnityBase.Runtime.Factories;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICurrencyManager
    {
        void PlayCollectCoin(IOwnerContext ownerContext, int amount, CoinIconTest prefab, Transform parent, Vector3 screenPos);
    }
}