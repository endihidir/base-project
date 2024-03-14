using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public class BattleCard : ICard
    {
        private readonly int _value;

        public BattleCard(int value) => _value = value;

        public int Play()
        {
            Debug.Log("Playing Battle card with value: " + _value);
            return _value;
        }
    }
}