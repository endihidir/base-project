using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    [CreateAssetMenu(fileName = "SwordFactory", menuName = "Game/Design Patterns/Factory/Weapon Factory/Sword", order = 10)]
    public class SwordFactory : WeaponFactory
    {
        private IWeapon _weapon;
        public override IWeapon CreateWeapon() => _weapon ??= new Sword();
    }
}