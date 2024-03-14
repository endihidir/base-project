using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.DesignPatterns.Factory
{
    public class Knight : MonoBehaviour
    {
        [SerializeField, Required] private WeaponFactory _weaponFactory;

        private IWeapon _weapon = IWeapon.CreateDefault();

        private void Start()
        {
            if (_weaponFactory is not null)
            {
                _weapon = _weaponFactory.CreateWeapon();
            }

            Attack();
        }

        private void Attack() => _weapon?.Attack();
    }
}
