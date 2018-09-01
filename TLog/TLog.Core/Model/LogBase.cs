using System;
using System.Collections.Generic;

namespace TLog.Core.Model
{
    /// <summary>
    /// 日志基础信息
    /// </summary>
    [Serializable]
    public class LogBase
    {
        /// <summary>
        /// 当前日志段
        /// </summary>
        public LogSpan LogSpan { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 自定义信息
        /// </summary>
        public Dictionary<string, object> CustomerInfo { get; set; }
    }
}
