using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    public class Soldier : MonoBehaviour
    {
        [SerializeField] private EquipmentFactory _equipmentFactory;

        private IWeapon _weapon;
        private IShield _shield;

        private void Start()
        {
            _weapon = _equipmentFactory.CreateWeapon();
            _shield = _equipmentFactory.CreateShield();

            Attack();
            Defend();
        }

        private void Attack() => _weapon.Attack();
        private void Defend() => _shield.Defend();
    }
}