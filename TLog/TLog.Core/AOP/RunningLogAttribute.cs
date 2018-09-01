using System;
using System.Collections.Generic;
using System.Reflection;
using ArxOne.MrAdvice.Advice;
using TLog.Core.Common;
using TLog.Core.ContextPropagation;
using TLog.Core.Log;
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
        /// <summary>
        /// 调用链
        /// </summary>
        private LogSpan _logSpan;

        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            // 跳过构造函数和属性(仅记录异常日志，运行日志不记录)
            if (context.TargetMethod.MemberType == MemberTypes.Constructor
                || context.TargetMethod.MemberType == MemberTypes.Property
                || context.TargetMethod.Name.StartsWith("set_")
                || context.TargetMethod.Name.StartsWith("get_"))
            {
                try
                {
                    _logSpan = LogSpan.GetCurrentLogSpan();
                    context.Proceed();
                }
                catch (Exception e)
                {
                    // 构造函数中，如果不出现异常，则调用链不用延长，出现异常后才延长调用链
                    var logTmp = LogSpan.Extend(LogContext.Current);
                    _logSpan.SpanChain = logTmp.SpanChain;

                    _logSpan.FunctionName = $"{context.TargetMethod.Name}---Type={context.TargetMethod.MemberType}";
                    _logSpan.ParamIn = GetInParam(context);
                    _logSpan.ParamOut = $"Exception:{e}";
                    _logSpan.SpendTime = (DateTime.Now - _logSpan.CreateTime).TotalMilliseconds;
                    LogManager.InnerException(e, "构造函数、属性初始化异常", _logSpan);
                    throw;
                }
                return;
            }

            // 普通函数，运行日志和异常日志都会记录
            try
            {
                _logSpan = LogSpan.Extend(LogContext.Current);
                context.Proceed();

                _logSpan.FunctionName = $"{context.TargetMethod.Name}---Type={context.TargetMethod.MemberType}";
                _logSpan.ParamIn = GetInParam(context);
                _logSpan.ParamOut = GetOutParam(context);
                _logSpan.SpendTime = (DateTime.Now - _logSpan.CreateTime).TotalMilliseconds;
                LogManager.InnerRunningLog(_logSpan);
            }
            catch (Exception e)
            {
                _logSpan.FunctionName = $"{context.TargetMethod.Name}---Type={context.TargetMethod.MemberType}";
                _logSpan.ParamIn = GetInParam(context);
                _logSpan.ParamOut = string.Empty;
                _logSpan.SpendTime = (DateTime.Now - _logSpan.CreateTime).TotalMilliseconds;
                LogManager.InnerException(e, "函数执行异常", _logSpan);

                if (context.TargetMethod.IsDefined(typeof(ThrowExAttribute), true))
                {
                    throw;
                }

                if (context.HasReturnValue)
                {
                    // 如果不抛异常到外层，则需要补上函数的返回值
                    var methodInfo = context.TargetMethod as MethodInfo;
                    if (methodInfo == null)
                    {
                        throw new Exception("日志组件自动补充返回值，未找到目标方法");
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
