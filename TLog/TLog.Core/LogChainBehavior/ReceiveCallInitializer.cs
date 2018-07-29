using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using TLog.Core.Model;

namespace TLog.Core.LogChainBehavior
{
    public class ReceiveCallInitializer : ICallContextInitializer
    {
        /// <summary>
        /// 是否返回调用上下文
        /// </summary>
        public bool IsReturnContext { get; set; }

        public ReceiveCallInitializer() : this(false)
        { }

        public ReceiveCallInitializer(bool isReturnContext)
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

            LogContext.Current = context;
            LogContext.Current.LogSpan.SpanChain += ".1";
            LogContext.Current.LogSpan.FunctionName = "";
            LogContext.Current.LogSpan.ParamIn = instanceContext.ToString();
            return LogContext.Current;
        }

        /// <summary>实现它来参与清理调用该操作的线程。</summary>
        /// <param name="correlationState">
        ///   从 <see cref="M:System.ServiceModel.Dispatcher.ICallContextInitializer.BeforeInvoke(System.ServiceModel.InstanceContext,System.ServiceModel.IClientChannel,System.ServiceModel.Channels.Message)" /> 方法返回的关联对象。
        /// </param>
        public void AfterInvoke(object correlationState)
        {
            if (this.IsReturnContext)
            {
                LogContext context = correlationState as LogContext;
                if (context != null)
                {
                    MessageHeader<LogContext> contextHeader = new MessageHeader<LogContext>(context);
                    OperationContext.Current.OutgoingMessageHeaders.Add(contextHeader.GetUntypedHeader(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace));
                }
            }

            LogContext.Current = null;
        }
    }
}
