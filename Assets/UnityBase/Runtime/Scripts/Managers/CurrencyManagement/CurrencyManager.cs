using UnityBase.Service;
using UnityBase.Runtime.Factories;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManager
    {
        public void Initialize() { }

        public void PlayCollectCoin(IOwnerContext ownerContext, int amount, CoinIconTest prefab, Transform parent, Vector3 screenPos)
        {
            if(!ownerContext.TryGetPresenter<ICoinPresenter>(out var presenter)) return;

            presenter.PlayCollect(amount, prefab, parent, screenPos);
        }

        public void Dispose() { }
    }
}