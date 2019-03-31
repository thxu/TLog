using System;
using System.Runtime.Serialization;

namespace TLog.SysLogCollector.Model
{
    /// <summary>
    /// SysLog
    /// </summary>
    [DataContract]
    public class SysLog
    {
        /// <summary>
        /// 全局追踪ID
        /// </summary>
        [DataMember]
        public string TraceId { get; set; }

        /// <summary>
        /// 追踪链
        /// </summary>
        [DataMember]
        public string SpanChain { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 耗时(单位 ms)
        /// </summary>
        [DataMember]
        public decimal SpendTime { get; set; }

        /// <summary>
        /// 执行函数名称
        /// </summary>
        [DataMember]
        public string FunctionName { get; set; }

        /// <summary>
        /// 函数入参
        /// </summary>
        [DataMember]
        public string ParamIn { get; set; }

        /// <summary>
        /// 函数回参
        /// </summary>
        [DataMember]
        public string ParamOut { get; set; }

        /// <summary>
        /// 当前IP
        /// </summary>
        [DataMember]
        public string Ip { get; set; }

        /// <summary>
        /// 当前模块名称
        /// </summary>
        [DataMember]
        public string AppName { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        [DataMember]
        public int LogLevel { get; set; }

        /// <summary>
        /// 自定义信息
        /// </summary>
        [DataMember]
        public string CustomerInfo { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [DataMember]
        public string ExceptionInfo { get; set; }
    }
}
