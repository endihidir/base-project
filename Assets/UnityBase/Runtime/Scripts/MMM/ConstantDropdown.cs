using System;
using UnityEngine;

namespace UnityBase.Scripts.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConstantDropdown : PropertyAttribute
    {
        public Type TargetType { get; }
        public ConstantDropdown(Type targetType) => TargetType = targetType;
    }
}
