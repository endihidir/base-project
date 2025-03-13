using UnityEngine;

namespace DependencyInjection
{
    public class Container : MonoBehaviour, IDependencyContainer
    {
        [Provide]
        public ServiceA ProvideServiceA() => new ServiceA();

        [Provide]
        public ServiceB ProvideServiceB() => new ServiceB();

        [Provide]
        public FactoryA ProvideFactoryA() => new FactoryA();
    }
}