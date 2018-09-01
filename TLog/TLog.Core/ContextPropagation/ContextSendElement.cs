using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace TLog.Core.ContextPropagation
{
    /// <summary>
    /// 上下文行为标签参数值
    /// </summary>
    public class ContextSendElement : BehaviorExtensionElement
    {
        /// <summary>
        /// 是否返回上下文
        /// </summary>
        [ConfigurationProperty("isReturnContext", DefaultValue = false)]
        public bool IsReturnContext
        {
            get
            {
                return (bool)this["isReturnContext"];
            }
            set
            {
                this["isReturnContext"] = value;
            }
        }


        /// <summary>基于当前配置设置来创建行为扩展。</summary>
        /// <returns>行为扩展。</returns>
        protected override object CreateBehavior()
        {
            return new ContextSendBehavior(IsReturnContext);
        }

        /// <summary>获取行为的类型。</summary>
        /// <returns>行为类型。</returns>
        public override Type BehaviorType
        {
            get { return typeof(ContextSendBehavior); }
        }
    }
}
