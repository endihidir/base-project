using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    [CreateAssetMenu(fileName = "BowFactory", menuName = "Game/Design Patterns/Factory/Weapon Factory/Bow", order = 10)]
    public class BowFactory : WeaponFactory
    {
        private IWeapon _weapon;
        public override IWeapon CreateWeapon() => _weapon ??= new Bow();
    }
}