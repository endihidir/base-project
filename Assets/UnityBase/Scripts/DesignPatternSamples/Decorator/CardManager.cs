using UnityBase.Singleton;
using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public class CardManager : SingletonBehaviour<CardManager>
    {
        public CardController selectedCard;
        
        private void Awake()
        {
            if (HasMultipleInstances()) return;
        }

        private void Start()
        {
            // Decorator class working system simple example!!!!!!!!!!!!
            
            /* //EXAMPLE 1 !!!!
            
            ICard originalCard = new BattleCard(5);
            
            CardDecorator decorator1 = new DamageDecorator(2);
            decorator1.Decorate(originalCard);
            
            ICard cardWithDecorator1 = decorator1;

            CardDecorator decorator2 = new HealthDecorator(3);
            decorator2.Decorate(cardWithDecorator1);
            
            Debug.Log(decorator2.Play());*/
            
            /* //EXAMPLE 2 !!!! (create Card{get;set;} property)
             
             ICard originalCard = new BattleCard(5);
            
            CardDecorator decorator1 = new DamageDecorator(2);
            decorator1.Card = originalCard;
            originalCard = decorator1;

            CardDecorator decorator2 = new HealthDecorator(3);
            decorator2.Card = originalCard;
            originalCard = decorator2;
            
            Debug.Log(originalCard.Play());*/
        }

        public void Decorate(CardController clickedCard)
        {
            if(selectedCard == clickedCard) return;

            if (selectedCard.Card is CardDecorator decorator)
            {
                Debug.Log("Decorating...");
                decorator.Decorate(clickedCard.Card);
                clickedCard.Card = decorator;
                selectedCard.MoveToAndDestroy(clickedCard.transform.position);
            }
            else
            {
                Debug.LogWarning("Cannot decorate card!");
            }
        }
    }
}