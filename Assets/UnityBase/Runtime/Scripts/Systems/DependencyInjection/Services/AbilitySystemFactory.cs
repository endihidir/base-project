using UnityEngine;

namespace DependencyInjection
{
    public class AbilitySystemFactory : MonoBehaviour, IDependencyContainer
    {
        [Provide]
        public AbilitySystemFactory ProvideAbilitySystemFactory()
        {
            return this;
        }
        
    }
}