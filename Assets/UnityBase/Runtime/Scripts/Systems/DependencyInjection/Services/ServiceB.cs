using UnityEngine;

namespace UnityBase.DI
{
    public class ServiceB
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceB.Initialize({message})");
        }
    }
}