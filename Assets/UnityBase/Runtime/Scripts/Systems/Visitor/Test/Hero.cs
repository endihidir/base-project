using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityBase.Visitor
{
    public class Hero : MonoBehaviour, IVisitable
    {
        public float moveSpeed = 5f;
        private List<IVisitable> _visitableComponents = new List<IVisitable>();

        private void Start()
        {
            _visitableComponents.Add(gameObject.GetOrAddComponent<HealthComponent>());
            _visitableComponents.Add(gameObject.GetOrAddComponent<ManaComponent>());
        }

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
            transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        }

        public void Accept(IVisitor visitor)
        {
            foreach (var component in _visitableComponents)
            {
                component.Accept(visitor);
            }
        }
    }
}