using UnityEngine;

namespace UnityBase.DI
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