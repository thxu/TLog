using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using TLog.Core.Model;

namespace TLog.Core.LogChainBehavior
{
    /// <summary>
    /// WCF客户端调用检查器
    /// </summary>
    public class ClientCallInspector : IClientMessageInspector
    {
        /// <summary>
        /// 是否返回调用上下文
        /// </summary>
        public bool IsReturnContext { get; set; }

        public ClientCallInspector() : this(false)
        { }

        public ClientCallInspector(bool isReturnContext)
        {
            IsReturnContext = isReturnContext;
        }

        /// <summary>在将请求消息发送到服务之前，启用消息的检查或修改。</summary>
        /// <param name="request">要发送给服务的消息。</param>
        /// <param name="channel">WCF 客户端对象通道。</param>
        /// <returns>
        ///   作为 <paramref>
        ///         <name xml:space="preserve">correlationState </name>
        ///     </paramref>
        ///     方法的 <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)" />参数返回的对象。
        ///    如果不使用关联状态，则为 <see langword="null" />。
        ///   最佳做法是将它设置为 <see cref="T:System.Guid" />，以确保没有两个相同的 <paramref>
        ///         <name>correlationState</name>
        ///     </paramref>
        ///     对象。
        /// </returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            string action = request.Headers.GetHeader<string>("Action", request.Headers[0].Namespace);
            LogContext.Current.LogSpan = Span.Extend(LogContext.Current.LogSpan);
            LogContext.Current.LogSpan.FunctionName = action;
            LogContext.Current.LogSpan.ParamIn = request.ToString();

            MessageHeader<LogContext> contextHeader = new MessageHeader<LogContext>(LogContext.Current);
            request.Headers.Add(contextHeader.GetUntypedHeader(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace));
            return null;
        }

        /// <summary>在收到回复消息之后将它传递回客户端应用程序之前，启用消息的检查或修改。</summary>
        /// <param name="reply">要转换为类型并交回给客户端应用程序的消息。</param>
        /// <param name="correlationState">关联状态数据。</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // 保存当前日志链节点，保证即使开发人员设置了返回日志上下文，也依然不会改变当前的日志链节点
            var spanTmp = LogContext.Current.LogSpan;
            if (IsReturnContext)
            {
                if (reply.Headers.FindHeader(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace) >= 0)
                {
                    LogContext context = reply.Headers.GetHeader<LogContext>(LogContext.ContextHeaderLocalName, LogContext.ContextHeaderNamespace);
                    LogContext.Current = context;
                    LogContext.Current.LogSpan = spanTmp;
                }
            }

            LogContext.Current.LogSpan.ParamOut = reply.ToString();
            LogContext.Current.LogSpan.TimeStampEnd = DateTime.Now.Ticks;

        }
    }
}
