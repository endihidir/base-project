using UnityEngine;

namespace UnityBase.Visitor
{
    [CreateAssetMenu(menuName = "Game/Design Patterns/Visitor/PowerUp", fileName = "PowerUp")]
    public class PowerUp : ScriptableObject, IVisitor
    {
        public int HealthBonus = 10;
        public int ManaBonus = 10;

        public void Visit<T>(T visitable) where T : IVisitable
        {
            if (visitable is HealthComponent healthComponent)
            {
                healthComponent.AddHealth(HealthBonus);
                Debug.Log("PowerUp.Visit(HealthComponent)");
            }
            else if (visitable is ManaComponent manaComponent)
            {
                manaComponent.AddMana(ManaBonus);
                Debug.Log("PowerUp.Visit(ManaComponent)");
            }
        }
        
        // If you want you could use REFLECTION like below statement example....
        
        /*public void Visit(object o)
        {
            MethodInfo visitedMethod = GetType().GetMethod("Visit", new[] { o.GetType() });

            if (visitedMethod is not null && visitedMethod != GetType().GetMethod("Visit", new[] { typeof(object)}))
            {
                visitedMethod.Invoke(this, new [] { o });
            }
            else
            {
                DefaultVisit(o);
            }
        }

        private void DefaultVisit(object o)
        {
            Debug.Log("PowerUp DefaultVisit");
        }

        public void Visit(HealthComponent healthComponent)
        {
            healthComponent.AddHealth(HealthBonus);
            Debug.Log("PowerUp.Visit(HealthComponent)");
        }
        
        public void Visit(ManaComponent manaComponent)
        {
            manaComponent.AddMana(ManaBonus);
            Debug.Log("PowerUp.Visit(ManaComponent)");
        }*/
    }
}