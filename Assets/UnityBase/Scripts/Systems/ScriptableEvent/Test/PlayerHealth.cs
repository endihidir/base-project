using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.ScriptableEvent
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private FloatEventChannel _healthBarChannel;

        [SerializeField] private float _currentHealth = 100f;

        private float _maxHealth = 100f;

        private void Start()
        {
            _healthBarChannel.Invoke(_currentHealth / _maxHealth);
        }

        [Button]
        public void TakeDamage(float val)
        {
            _currentHealth -= val;

            if (_currentHealth < 0) _currentHealth = 0f;

            _healthBarChannel.Invoke(_currentHealth / _maxHealth);
        }
    }
}
