using System;
using TLog.Core.Common;

namespace TLog.Core.Model
{
    [Serializable]
    public class Span
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
        /// 开始执行时间戳
        /// </summary>
        public long TimeStampBegin { get; set; }

        /// <summary>
        /// 执行结束时间戳
        /// </summary>
        public long TimeStampEnd { get; set; }

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

        private Span()
        {

        }

        /// <summary>
        /// 初始化日志链的头节点
        /// </summary>
        /// <returns>日志节点</returns>
        public static Span IniHeadSpan()
        {
            Span res = new Span();
            res.TraceId = DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString("N");
            res.SpanChain = "1";
            res.TimeStampBegin = DateTime.Now.Ticks;
            res.TimeStampEnd = DateTime.MinValue.Ticks;
            res.FunctionName = string.Empty;
            res.ParamIn = string.Empty;
            res.ParamOut = string.Empty;
            res.Ip = Common.Common.GetLocalIPAddress();
            return res;
        }

        /// <summary>
        /// 扩展一个新日志节点
        /// </summary>
        /// <param name="span">日志尾节点</param>
        /// <returns>新日志节点</returns>
        public static Span Extend(Span span)
        {
            Span node = new Span();
            node.TraceId = span.TraceId;
            node.SpanChain = span.SpanChain.AddSpanChain();
            node.TimeStampBegin = DateTime.Now.Ticks;
            node.TimeStampEnd = DateTime.MinValue.Ticks;
            node.FunctionName = string.Empty;
            node.ParamIn = string.Empty;
            node.ParamOut = string.Empty;
            node.Ip = Common.Common.GetLocalIPAddress();
            return node;
        }
    }
}
