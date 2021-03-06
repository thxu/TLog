﻿using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace TLog.Core.ContextPropagation
{
    /// <summary>
    /// 上下文发送行为
    /// </summary>
    public class ContextSendBehavior : IEndpointBehavior
    {
        /// <summary>
        /// 是否返回调用上下文
        /// </summary>
        public bool IsReturnContext { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ContextSendBehavior() : this(false){ }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isReturnContext">是否返回上下文</param>
        public ContextSendBehavior(bool isReturnContext)
        {
            IsReturnContext = isReturnContext;
        }

        /// <summary>实现此方法可以确认终结点是否满足某些设定条件。</summary>
        /// <param name="endpoint">要验证的终结点。</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>实现此方法可以在运行时将数据传递给绑定，从而支持自定义行为。</summary>
        /// <param name="endpoint">要修改的终结点。</param>
        /// <param name="bindingParameters">绑定元素支持该行为所需的对象。</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>在终结点范围内实现服务的修改或扩展。</summary>
        /// <param name="endpoint">公开协定的终结点。</param>
        /// <param name="endpointDispatcher">要修改或扩展的终结点调度程序。</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (var operation in endpointDispatcher.DispatchRuntime.Operations)
            {
                operation.CallContextInitializers.Add(new ContextReceiveInitializer(IsReturnContext));
            }
        }

        /// <summary>在终结点范围内实现客户端的修改或扩展。</summary>
        /// <param name="endpoint">要自定义的终结点。</param>
        /// <param name="clientRuntime">要自定义的客户端运行时。</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ContextSendInspector(IsReturnContext));
        }
    }
}
