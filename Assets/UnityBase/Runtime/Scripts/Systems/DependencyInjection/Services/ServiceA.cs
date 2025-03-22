using UnityEngine;

namespace UnityBase.DI
{
    public class ServiceA
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceA.Initialize({message})");
        }
    }
}