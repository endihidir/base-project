using UnityEngine;

namespace UnityBase.Mvc.Architecture
{
    public class CoinView : MonoBehaviour, ICoinView
    {
        public void UpdateCoinsDisplay(int coins)
        {
            
        }
    }

    public interface ICoinView
    {
        void UpdateCoinsDisplay(int coins);
    }

    public interface ICoinService
    {
        ICoinModel Load();
        void Save(ICoinModel coinModel);
    }
}