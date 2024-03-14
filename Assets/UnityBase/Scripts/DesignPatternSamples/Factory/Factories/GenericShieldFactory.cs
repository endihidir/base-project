using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    [CreateAssetMenu(fileName = "GenericShieldFactory", menuName = "Game/Design Patterns/Factory/Shield Factory/Generic", order = 10)]
    public class GenericShieldFactory : ShieldFactory
    {
        private IShield _shield;
        public override IShield CreateShield() => _shield ??= new Shield();
    }
}