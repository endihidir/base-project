using UnityEngine;

namespace UnityBase.Visitor
{
    public class PowerUpObject : MonoBehaviour
    {
        public PowerUp PowerUp;

        private void OnTriggerEnter(Collider other)
        {
            var isVisitableFound = other.TryGetComponent<IVisitable>(out var visitable);

            if (isVisitableFound)
            {
                visitable.Accept(PowerUp);
                Destroy(gameObject);
            }
        }
    }
}