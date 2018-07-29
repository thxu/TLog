using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace TLog.Core.Common
{
    public static class Common
    {
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns>本机IP</returns>
        public static string GetLocalIPAddress()
        {
            StringBuilder buid = new StringBuilder();

            string hostName = Dns.GetHostName();//本机名   
            buid.Append(hostName + ";");
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   

            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    buid.Append(ip + ";");
                }
            }
            return buid.ToString();
        }

        /// <summary>
        /// object序列化JSON字符串扩展方法
        /// </summary>
        /// <param name="obj">object及其子类对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            try
            {
                JsonSerializerSettings settting = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" };
                return JsonConvert.SerializeObject(obj, settting);
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 日志链编号自动加1
        /// </summary>
        /// <param name="currChainNo">日志链编号</param>
        /// <returns>新日志链编号</returns>
        public static string AddSpanChain(this string currChainNo)
        {
            if (string.IsNullOrWhiteSpace(currChainNo))
            {
                return "1";
            }
            string[] tmp = currChainNo.Split('.');
            int val = Convert.ToInt32(tmp[tmp.Length - 1]) + 1;
            tmp[tmp.Length - 1] = val.ToString();
            return string.Join(".", tmp);
        }
    }
}
