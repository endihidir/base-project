using System;
using UnityEngine;

namespace UnityBase.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectAttribute : PropertyAttribute
    {
        
    }
}