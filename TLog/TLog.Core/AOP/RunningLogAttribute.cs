using System;
using System.Collections.Generic;
using System.Reflection;
using ArxOne.MrAdvice.Advice;
using TLog.Core.Common;
using TLog.Core.LogChainBehavior;
using TLog.Core.Model;

namespace TLog.Core.AOP
{
    /// <summary>
    /// 运行日志
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RunningLogAttribute : Attribute, IMethodAdvice
    {
        private Span _logSpan;

        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            try
            {
                _logSpan = Span.Extend(LogContext.Current.LogSpan);
                _logSpan.FunctionName = context.TargetMethod.Name;
                _logSpan.ParamIn = GetInParam(context);
                
                context.Proceed();

                _logSpan.ParamOut = GetOutParam(context);
                _logSpan.TimeStampEnd = DateTime.Now.Ticks;
                
            }
            catch (Exception e)
            {
                _logSpan.ParamOut = $"Exception:{e}";
                _logSpan.TimeStampEnd = DateTime.Now.Ticks;

                // 记异常日志

                if (context.TargetMethod.IsDefined(typeof(ThrowExAttribute), true))
                {
                    throw;
                }

                if (context.HasReturnValue)
                {
                    var methodInfo = context.TargetMethod as MethodInfo;
                    if (methodInfo == null)
                    {
                        throw new Exception("未找到目标方法");
                    }
                    context.ReturnValue = DefaultForType(methodInfo.ReturnType);
                }
            }
        }

        /// <summary>
        /// 获取入参
        /// </summary>
        /// <param name="context">函数调用上下文</param>
        /// <returns>输入参数json字符串</returns>
        private string GetInParam(MethodAdviceContext context)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            IList<object> arguments = context.Arguments;
            ParameterInfo[] parameters = context.TargetMethod.GetParameters();
            for (int i = 0; arguments != null && i < arguments.Count; i++)
            {
                res.Add(parameters[i].Name, arguments[i].ToJson());
            }
            return res.ToJson();
        }

        /// <summary>
        /// 获取返回参数
        /// </summary>
        /// <param name="context">函数调用上下文</param>
        /// <returns>返回参数</returns>
        private string GetOutParam(MethodAdviceContext context)
        {
            if (!context.HasReturnValue)
            {
                return null;
            }
            return context.ReturnValue.ToString();
        }

        /// <summary>
        /// 生成类型默认值
        /// </summary>
        /// <param name="targetType">类型</param>
        /// <returns>类型默认值</returns>
        private static object DefaultForType(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
