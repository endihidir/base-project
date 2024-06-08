using UnityBase.ManagerSO;
using UnityBase.Service;

namespace UnityBase.Manager
{
    public class CurrencyManager : ICurrencyManager, IAppBootService
    {
        public CurrencyManager(ManagerDataHolderSO managerDataHolderSo)
        {
            var currencyManagerData = managerDataHolderSo.currencyManagerSo;
        }

        public void Initialize() { }
        public void Dispose() { }
    }
}