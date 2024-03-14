using System;
using UnityEngine;

namespace UnityBase.DesignPatterns.Strategy
{
    public class Hero : MonoBehaviour
    {
        [SerializeField] private SpellStrategy[] _spells;

        public event Action<int> OnButtonPressed;

        private void OnEnable()
        {
            OnButtonPressed += CastSpell;
        }

        private void OnDisable()
        {
            OnButtonPressed -= CastSpell;
        }

        private void CastSpell(int index)
        {
            if (_spells.Length <= index) return;

            _spells[index].CastSpell(transform);
        }
    }
}