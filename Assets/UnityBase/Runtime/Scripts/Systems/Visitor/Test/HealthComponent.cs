using UnityEngine;

namespace UnityBase.Visitor
{
    public class HealthComponent : MonoBehaviour, IVisitable
    {
        [SerializeField] private int _health = 100;
        
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            Debug.Log("HealthComponent Accept");
        }

        public void AddHealth(int health)
        {
            _health += health;
        }
    }
}