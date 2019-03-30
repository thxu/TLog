using System;

namespace TLog.Core.AOP
{
    /// <summary>
    /// 捕获异常
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CatchExceptionAttribute : Attribute
    {
    }
}
