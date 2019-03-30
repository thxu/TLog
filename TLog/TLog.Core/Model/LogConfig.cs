using System;
using TLog.Core.Log;

namespace TLog.Core.Model
{
    /// <summary>
    /// 配置信息
    /// </summary>
    internal static class LogConfig
    {
        /// <summary>
        /// 日志提供者程序集(默认为Go.Log.Core程序集)
        /// </summary>
        public static string LogProviderAssembly { get; private set; }

        /// <summary>
        /// 日志提供者类型（默认为Go.Log.Core.Log.TxtLogger）
        /// </summary>
        public static string LogProviderType { get; private set; }

        /// <summary>
        /// 日志等级(0=全部，1=runninglog,2=info,4=warning,8=error,16=fatal)，如需要记录多个等级请将各等级按位或
        /// </summary>
        public static int LogLevel { get; private set; }

        /// <summary>
        /// 丢弃比例，设置此值后程序将按照设定值随机丢弃日志记录,例如，0.3=丢弃30%的日志
        /// </summary>
        public static double DropRate { get; private set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public static string AppName { get; set; }

        static LogConfig()
        {
            RefreshConfig();
            ConfigMonitor.ConfigModifyInfoEvent += RefreshConfig;
        }

        /// <summary>
        /// 刷新设置
        /// </summary>
        private static void RefreshConfig()
        {
            try
            {
                LogProviderAssembly = ConfigMonitor.ParamConfig.AppSettings.Settings["LogProviderAssembly"].Value.Trim();
                if (string.IsNullOrWhiteSpace(LogProviderAssembly))
                {
                    LogProviderAssembly = "TLog.Core";
                }
            }
            catch (Exception)
            {
                LogProviderAssembly = "TLog.Core";
            }

            try
            {
                LogProviderType = ConfigMonitor.ParamConfig.AppSettings.Settings["LogProviderType"].Value.Trim();
                if (string.IsNullOrWhiteSpace(LogProviderType))
                {
                    LogProviderType = "TLog.Core.Log.TxtLogger";
                }
            }
            catch (Exception)
            {
                LogProviderType = "TLog.Core.Log.TxtLogger";
            }

            try
            {
                int.TryParse(ConfigMonitor.ParamConfig.AppSettings.Settings["LogLevel"].Value.Trim(), out var logLevelTmp);
                LogLevel = logLevelTmp;
            }
            catch (Exception)
            {
                LogLevel = 0;
            }

            try
            {
                double rate;
                double.TryParse(ConfigMonitor.ParamConfig.AppSettings.Settings["DropRate"].Value.Trim(), out rate);
                DropRate = rate;
            }
            catch (Exception)
            {
                DropRate = 0;
            }

            try
            {
                AppName = ConfigMonitor.ParamConfig.AppSettings.Settings["AppName"].Value.Trim();
                if (string.IsNullOrWhiteSpace(AppName))
                {
                    AppName = string.Empty;
                }
            }
            catch (Exception)
            {
                AppName = string.Empty;
            }
        }
    }
}
