using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using TLog.Core.Log;
using TLog.Core.Model;

namespace TLog.Core.ContextPropagation
{
    /// <summary>
    /// 上下文接收处理
    /// </summary>
    public class ContextReceiveInitializer : ICallContextInitializer
    {
        /// <summary>
        /// 是否返回调用上下文
        /// </summary>
        public bool IsReturnContext { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ContextReceiveInitializer() : this(false) { }

        /// <summary>
        /// 日志段
        /// </summary>
        //private LogSpan _logSpan;

        private Dictionary<string, LogSpan> _logSpanDic = new Dictionary<string, LogSpan>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isReturnContext">是否返回上下文</param>
        public ContextReceiveInitializer(bool isReturnContext)
        {
            this.IsReturnContext = isReturnContext;
        }

        /// <summary>实现它来参与初始化操作线程。</summary>
        /// <param name="instanceContext">操作的服务实例。</param>
        /// <param name="channel">客户端通道。</param>
        /// <param name="message">传入消息。</param>
        /// <returns>
        ///   作为 <see cref="M:System.ServiceModel.Dispatcher.ICallContextInitializer.AfterInvoke(System.Object)" /> 方法的参数传回的关联对象。
        /// </returns>
        public object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message)
        {
            LogContext context = message.Headers.GetHeader<LogContext>(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace);
            string action = message.Headers.GetHeader<string>("Action", message.Headers[0].Namespace);

            LogContext.Current = context;
            LogContext.Current.SpanChain += ".0";

            var _logSpan = LogSpan.Extend(LogContext.Current);
            _logSpanDic.Add(_logSpan.TraceId, _logSpan);
            _logSpan.FunctionName = "WCF Service :" + action;
            _logSpan.ParamIn = message.ToString();
            return LogContext.Current;
        }

        /// <summary>实现它来参与清理调用该操作的线程。</summary>
        /// <param name="correlationState">
        ///   从 <see cref="M:System.ServiceModel.Dispatcher.ICallContextInitializer.BeforeInvoke(System.ServiceModel.InstanceContext,System.ServiceModel.IClientChannel,System.ServiceModel.Channels.Message)" /> 方法返回的关联对象。
        /// </param>
        public void AfterInvoke(object correlationState)
        {
            LogContext context = correlationState as LogContext;
            if (context != null)
            {
                var _logSpan = _logSpanDic[context.TraceId];
                _logSpan.SpendTime = (DateTime.Now - _logSpan.CreateTime).TotalMilliseconds;
                LogManager.InnerRunningLog(_logSpan);
                _logSpanDic.Remove(_logSpan.TraceId);
                if (this.IsReturnContext)
                {
                    MessageHeader<LogContext> contextHeader = new MessageHeader<LogContext>(context);
                    OperationContext.Current.OutgoingMessageHeaders.Add(contextHeader.GetUntypedHeader(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace));
                }
            }
            

            LogContext.Current = null;
        }
    }
}
