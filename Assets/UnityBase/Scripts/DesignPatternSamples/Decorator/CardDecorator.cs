using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public abstract class CardDecorator : ICard
    {
        protected ICard _card;
        protected readonly int _value;
        protected CardDecorator(int value) => _value = value;
        public void Decorate(ICard card)
        {
            if (ReferenceEquals(this, card))
            {
                //throw new InvalidOperationException("Cannot decorate self");
                Debug.LogError("Cannot decorate self");
                return;
            }
            
            //These logs help you to remember and understand what the point is....
            
            //Debug.Log(_card);

            if (_card is CardDecorator decorator)
            {
                // Other card data(clicked ICard (card)) is empty at the beginning of the second recursive loop.
                // So that Loop pass to else statement.
                
                //Debug.Log("a");
                decorator.Decorate(card);
            }
            else
            {
                //Debug.Log("b");
                _card = card;
            }
        }

        public virtual int Play()
        {
            Debug.Log("Playing Decorator card with value: " + _value);
            return _card?.Play() + _value ?? _value;
        }
    }

    public class DamageDecorator : CardDecorator
    {
        public DamageDecorator(int value) : base(value)
        {

        }

        public override int Play()
        {
            Debug.Log("Doubling damage of decorated card: " + _value);
            return _card?.Play() * 2 ?? 0;
        }
    }

    public class HealthDecorator : CardDecorator
    {
        public HealthDecorator(int value) : base(value)
        {

        }

        public override int Play()
        {
            Debug.Log("Playing health card decorator card with value: " + _value);
            HealPlayer();
            return _card?.Play() ?? 0;
        }

        private void HealPlayer()
        {

        }
    }
}