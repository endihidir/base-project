using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    [CreateAssetMenu(fileName = "EquipmentFactory", menuName = "Game/Design Patterns/Factory/Equipment Factory", order = 10)]
    public class EquipmentFactory : ScriptableObject
    {
        public WeaponFactory weaponFactory;
        public ShieldFactory shieldFactory;

        public IWeapon CreateWeapon() => weaponFactory?.CreateWeapon() ?? IWeapon.CreateDefault();
        public IShield CreateShield() => shieldFactory?.CreateShield() ?? IShield.CreateDefault();
    }
}