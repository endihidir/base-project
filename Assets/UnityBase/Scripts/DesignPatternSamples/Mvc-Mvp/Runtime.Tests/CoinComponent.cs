using UnityBase.Mvc.Architecture;
using UnityBase.Visitor;
using UnityEngine;

namespace UnityBase.Mvc.RuntimeTest
{
    public class CoinComponent : MonoBehaviour, IVisitable
    {
        public ICoinController controller;
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}