using System;

namespace TLog.Core.AOP
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ThrowExAttribute : Attribute
    {
    }
}
