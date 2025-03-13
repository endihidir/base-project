using UnityEngine;

namespace UnityBase.Visitor
{
    public class ManaComponent : MonoBehaviour, IVisitable
    {
        [SerializeField] private int _mana = 100;
        
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            Debug.Log("ManaComponent Accept");
        }

        public void AddMana(int mana)
        {
            _mana += mana;
        }
    }
}