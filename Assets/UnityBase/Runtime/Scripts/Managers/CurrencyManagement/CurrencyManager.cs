using UnityBase.BootService;
using UnityBase.Service;
using UnityBase.Runtime.Behaviours;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManager, IAppBootService
    {
        private IOwnerContext _coinCtx;
        private ICoinController _coinController;

        public void Initialize() { }

        public void BindCoinContext(IOwnerContext context)
        {
            _coinCtx = context;
            
            if (!_coinCtx.TryGetController(out _coinController))
            {
                _coinController = _coinCtx.CreateController<CoinController>();
            }
            
            _coinController.Initialize(_coinCtx);
        }

        public void UnbindCoinContext(IOwnerContext context)
        {
            if (_coinCtx != context) return;
            
            _coinController?.Dispose();
            _coinController = null;
            _coinCtx = null;
        }

        public void PlayCollect(int amount, CoinIconTest prefab, Transform parent, Vector3 screenPos)
        {
            if (_coinController == null)
            {
                DebugLogger.LogError("[CurrencyManager] CoinController not ready.");
                return;
            }

            _coinController.PlayCollect(amount, prefab, parent, screenPos);
        }

        public void Dispose() { }
    }
}