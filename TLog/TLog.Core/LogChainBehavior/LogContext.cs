using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using TLog.Core.Model;

namespace TLog.Core.LogChainBehavior
{
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

        public new object this[string key]
        {
            get => base[key];
            set
            {
                EnsureSerializable(value);
                base[key] = value;
            }
        }

        public Span LogSpan
        {
            get
            {
                if (!Keys.Contains("_Span"))
                {
                    this["_Span"] = Span.IniHeadSpan();
                }

                return (Span)this["_Span"];
            }
            set
            {
                this["_Span"] = value;
            }
        }

        public static LogContext Current
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var logContext = HttpContext.Current.Items[CallContextKey] as LogContext;
                    if (logContext == null)
                    {
                        logContext = new LogContext();
                        logContext.LogSpan = Span.IniHeadSpan();
                        HttpContext.Current.Items.Add(CallContextKey, logContext);
                    }

                    return logContext;
                }
                if (CallContext.GetData(CallContextKey) == null)
                {
                    LogContext logContext = new LogContext();
                    logContext.LogSpan = Span.IniHeadSpan();
                    CallContext.LogicalSetData(CallContextKey, logContext);
                }

                return CallContext.GetData(CallContextKey) as LogContext;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items.Add(CallContextKey, value);
                }
                CallContext.LogicalSetData(CallContextKey, value);
            }
        }
    }
}
