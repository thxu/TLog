using System;
using System.Reflection;
using ArxOne.MrAdvice.Advice;
using TLog.Core.ContextPropagation;
using TLog.Core.Log;
using TLog.Core.Model;

namespace TLog.Core.AOP
{
    /// <summary>
    /// 捕获异常
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CatchExceptionAttribute : Attribute
    {
        /// <summary>
        /// 调用链
        /// </summary>
        private LogSpan _logSpan;

        /// <summary>
        /// 是否记录finally日志
        /// </summary>
        private readonly bool _isLogFinally;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isLogFinally"></param>
        public CatchExceptionAttribute(bool isLogFinally = false)
        {
            _isLogFinally = isLogFinally;
        }

        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            try
            {
                var paramIn = RunningLogAttribute.GetInParam(context);
                _logSpan = LogSpan.Extend(LogContext.Current);
                _logSpan.FunctionName = $"{context.TargetMethod.Name}";
                _logSpan.ParamIn = paramIn;

                context.Proceed();
            }
            catch (Exception e)
            {
                _logSpan.ParamOut = $"Exception:{e}";
                _logSpan.SpendTime = (DateTime.Now - _logSpan.CreateTime).TotalMilliseconds;
                LogManager.InnerException(e, "函数执行异常", _logSpan);

                if (context.HasReturnValue)
                {
                    // 如果不抛异常到外层，则需要补上函数的返回值
                    var methodInfo = context.TargetMethod as MethodInfo;
                    if (methodInfo == null)
                    {
                        throw new Exception("日志组件自动补充返回值，未找到目标方法");
                    }
                    context.ReturnValue = DefaultForType(methodInfo.ReturnType, e.Message);
                }
            }
            finally
            {
                if (_isLogFinally)
                {
                    string res = "";
                    if (context.HasReturnValue)
                    {
                        res = Newtonsoft.Json.JsonConvert.SerializeObject(context.ReturnValue);
                    }
                    LogManager.InnerInfo(_logSpan, res);
                }
            }
        }

        /// <summary>
        /// 生成类型默认值
        /// </summary>
        /// <param name="targetType">类型</param>
        /// <param name="msg">Result类型的错误信息</param>
        /// <returns>类型默认值</returns>
        public static object DefaultForType(Type targetType, string msg = null)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
