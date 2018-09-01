using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using TLog.Core.Model;

namespace TLog.Core.ContextPropagation
{
    /// <summary>
    /// 日志上下文
    /// </summary>
    public class LogContext : Dictionary<string, object>
    {
        private const string CallContextKey = "__LogContext";
        internal const string ContextHeaderLocalName = "__LogContext";
        internal const string ContextHeaderNamespace = "TLog.Core.com";

        private void EnsureSerializable(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (!value.GetType().IsSerializable)
            {
                throw new ArgumentException($"The argument of the type \"{value.GetType().FullName}\" is not serializable!");
            }
        }

        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>val</returns>
        public new object this[string key]
        {
            get => base[key];
            set
            {
                EnsureSerializable(value);
                base[key] = value;
            }
        }

        /// <summary>
        /// 日志段
        /// </summary>
        //public LogSpan LogSpan
        //{
        //    get
        //    {
        //        if (!Keys.Contains("_Span"))
        //        {
        //            this["_Span"] = LogSpan.IniHeadSpan();
        //        }

        //        return (LogSpan)this["_Span"];
        //    }
        //    set
        //    {
        //        this["_Span"] = value;
        //    }
        //}

        /// <summary>
        /// 全局追踪id
        /// </summary>
        public string TraceId
        {
            get
            {
                if (!Keys.Contains("_TraceId"))
                {
                    this["_TraceId"] = LogSpan.CreateNewTraceId();
                }

                return (string)this["_TraceId"];
            }
            set
            {
                this["_TraceId"] = value;
            }
        }

        /// <summary>
        /// 当前日志链
        /// </summary>
        public string SpanChain
        {
            get
            {
                if (!Keys.Contains("_SpanChain"))
                {
                    this["_SpanChain"] = "1";
                }

                return (string)this["_SpanChain"];
            }
            set
            {
                this["_SpanChain"] = value;
            }
        }

        /// <summary>
        /// 获取或设置当前上下文
        /// </summary>
        public static LogContext Current
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var logContext = HttpContext.Current.Items[CallContextKey] as LogContext;
                    if (logContext == null)
                    {
                        logContext = new LogContext
                        {
                            TraceId = LogSpan.CreateNewTraceId(),
                            SpanChain = "0"
                        };
                        HttpContext.Current.Items.Add(CallContextKey, logContext);
                    }

                    return logContext;
                }
                if (CallContext.GetData(CallContextKey) == null)
                {
                    LogContext logContext = new LogContext
                    {
                        TraceId = LogSpan.CreateNewTraceId(),
                        SpanChain = "0"
                    };
                    CallContext.LogicalSetData(CallContextKey, logContext);
                }

                return CallContext.GetData(CallContextKey) as LogContext;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Items[CallContextKey] == null)
                    {
                        HttpContext.Current.Items.Add(CallContextKey, value);
                    }
                    else
                    {
                        HttpContext.Current.Items[CallContextKey] = value;
                    }
                }
                CallContext.LogicalSetData(CallContextKey, value);
            }
        }
    }
}
