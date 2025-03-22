using UnityEngine;

namespace UnityBase.DI
{
    public class ClassA : MonoBehaviour
    {
        private ServiceA _serviceA;
        
        [Inject]
        public ServiceB ServiceB { get; set; }
        
        [Inject]
        public void Init(ServiceA serviceA)
        {
            _serviceA = serviceA;
        }

        private void Awake()
        {
            _serviceA.Initialize("Yup");
            
            ServiceB?.Initialize("Yep");
            
            Debug.Log("----------------------------------");
        }
    }
}