using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace TLog.Core.LogChainBehavior
{
    public class ClientBehaviorElement : BehaviorExtensionElement
    {

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
            return new ClientBehavior(IsReturnContext);
        }

        /// <summary>获取行为的类型。</summary>
        /// <returns>行为类型。</returns>
        public override Type BehaviorType
        {
            get { return typeof(ClientBehavior); }
        }
    }
}
