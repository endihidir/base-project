using UnityEngine;

namespace UnityBase.DesignPatterns.Strategy
{
    public abstract class SpellStrategy : ScriptableObject
    {
        public abstract void CastSpell(Transform origin);
    }
}