namespace UnityBase.DI
{
    public class FactoryA
    {
        private ServiceA _serviceA;

        public ServiceA CreateServiceA() => _serviceA ??= new ServiceA();
    }
}