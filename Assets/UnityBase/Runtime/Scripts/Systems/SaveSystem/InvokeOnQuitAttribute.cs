using System;

namespace UnityBase.SaveSystem
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class InvokeOnQuitAttribute : Attribute { }
}