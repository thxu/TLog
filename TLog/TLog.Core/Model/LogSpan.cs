using System;
using TLog.Core.Common;
using TLog.Core.ContextPropagation;

namespace TLog.Core.Model
{
    /// <summary>
    /// 日志片段
    /// </summary>
    [Serializable]
    public class LogSpan
    {
        /// <summary>
        /// 全局追踪ID
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// 追踪链
        /// </summary>
        public string SpanChain { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 耗时(单位 ms)
        /// </summary>
        public double SpendTime { get; set; }

        /// <summary>
        /// 执行函数名称
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// 函数入参
        /// </summary>
        public string ParamIn { get; set; }

        /// <summary>
        /// 函数回参
        /// </summary>
        public string ParamOut { get; set; }

        /// <summary>
        /// 当前IP
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 当前模块名称
        /// </summary>
        public string AppName { get; set; }

        private LogSpan()
        {

        }

        /// <summary>
        /// 生成一个新的追踪id
        /// </summary>
        /// <returns></returns>
        public static string CreateNewTraceId(string key = "")
        {
            return DateTime.Now.ToString("yyMMddHHmmss") + Guid.NewGuid().ToString("N") + key;
        }

        /// <summary>
        /// 扩展一个新日志节点
        /// </summary>
        /// <param name="context">日志上下文</param>
        /// <returns>新日志节点</returns>
        public static LogSpan Extend(LogContext context)
        {
            LogSpan node = new LogSpan
            {
                TraceId = context.TraceId,
                SpanChain = context.SpanChain.AddSpanChain(),
                CreateTime = DateTime.Now,
                SpendTime = -1,
                FunctionName = string.Empty,
                ParamIn = string.Empty,
                ParamOut = string.Empty,
                Ip = Common.Common.GetLocalIPAddress(),
                AppName = LogConfig.AppName
            };

            LogContext.Current.TraceId = context.TraceId;
            LogContext.Current.SpanChain = node.SpanChain;
            return node;
        }

        /// <summary>
        /// 获取日志节点
        /// </summary>
        /// <returns>新日志节点</returns>
        public static LogSpan GetCurrentLogSpan()
        {
            LogSpan node = new LogSpan
            {
                TraceId = LogContext.Current.TraceId,
                SpanChain = LogContext.Current.SpanChain,
                CreateTime = DateTime.Now,
                SpendTime = -1,
                FunctionName = string.Empty,
                ParamIn = string.Empty,
                ParamOut = string.Empty,
                Ip = Common.Common.GetLocalIPAddress(),
                AppName = LogConfig.AppName
            };
            return node;
        }
    }
}
