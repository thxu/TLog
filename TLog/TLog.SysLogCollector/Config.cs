using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace TLog.SysLogCollector
{
    public class Config
    {
        private static string _strConnectionString
        {
            get
            {
                var tmp = ConfigurationManager.AppSettings["ElasticSearchConStr"] ?? "http://10.0.0.9:9200/";
                if (string.IsNullOrWhiteSpace(tmp))
                {
                    tmp = "http://10.0.0.9:9200/";
                }
                return tmp;
            }
        }

        public static Uri Node => new Uri(_strConnectionString);

        public static ConnectionSettings ConnectionSettingsNest => new ConnectionSettings(Node).DefaultIndex("accounts");

        public static ElasticClient GetNestClient()
        {
            return new ElasticClient(ConnectionSettingsNest);
        }

        /// <summary>
        /// 程序执行间隔，单位（秒）
        /// </summary>
        public static int Interval
        {
            get
            {
                var tmp = ConfigurationManager.AppSettings["Interval"];
                if (string.IsNullOrWhiteSpace(tmp))
                {
                    tmp = "10";
                }
                return Convert.ToInt32(tmp);
            }
        }

        /// <summary>
        /// 待归集的日志文件夹集合（多个用|隔开）
        /// </summary>
        public static List<string> Logs
        {
            get
            {
                var tmp = ConfigurationManager.AppSettings["LogFolders"];
                if (string.IsNullOrWhiteSpace(tmp))
                {
                    return new List<string>();
                }

                return tmp.Split('|').ToList();
            }
        }
    }
}
