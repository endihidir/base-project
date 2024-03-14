using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.DesignPatterns.Strategy
{
    [CreateAssetMenu(fileName = "ShieldSpellStrategy", menuName = "Game/Design Patterns/Strategy/Spells/ShieldSpellStrategy")]
    public class ShieldSpellStrategy : SpellStrategy
    {
        public GameObject shieldPrefab;
        public float duration = 10f;

        public override void CastSpell(Transform origin)
        {
            var shield = Instantiate(shieldPrefab, origin.position.With(y: -1.5f), Quaternion.identity, origin);
            Destroy(shield, duration);
        }
    }
}