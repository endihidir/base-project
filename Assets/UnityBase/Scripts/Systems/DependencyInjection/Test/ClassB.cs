using UnityEngine;

namespace DependencyInjection
{
    public class ClassB : MonoBehaviour
    {
        [Inject] private ServiceA _serviceA;
        
        [Inject] private ServiceB _serviceB;
        
        [Inject] private FactoryA _factoryA;

        [Inject, SerializeField] private AbilitySystemFactory _factory;

        [SerializeField] private MonoBehaviour _type;
        

        private void Awake()
        {
            _serviceA.Initialize("Nein");
            _serviceB.Initialize("No");
            
            var a = _factoryA.CreateServiceA();
            a.Initialize("This is created by factory A");
            
            Debug.Log("----------------------------------");
        }
    }
}