using UnityEngine;

namespace UnityBase.DI
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