using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public enum CardType
    {
        Battle,
        Damage,
        Health
    }
    
    [CreateAssetMenu(fileName = "New Card", menuName = "Game/Design Patterns/Decorator/CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        public int value = 10;
        public CardType type = CardType.Battle;
    }

    public static class CardFactory
    {
        public static ICard Crate(CardDefinition definition)
        {
            return definition.type switch
            {
                CardType.Damage => new DamageDecorator(definition.value),
                CardType.Health => new HealthDecorator(definition.value),
                _ => new BattleCard(definition.value)
            };
        }
    }
}