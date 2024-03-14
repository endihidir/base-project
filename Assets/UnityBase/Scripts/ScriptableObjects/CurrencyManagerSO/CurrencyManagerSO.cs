using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/CurrencyManagerData")]
    public class CurrencyManagerSO : ScriptableObject
    {
        public int startCoinAmount = 0;
        public void Initialize()
        {

        }
    }
}