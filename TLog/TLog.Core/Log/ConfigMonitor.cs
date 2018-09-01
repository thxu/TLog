using System;
using System.Configuration;
using System.IO;

namespace TLog.Core.Log
{
    /// <summary>
    /// 配置监控器，主要用于监控BetterLogParams
    /// </summary>
    internal class ConfigMonitor
    {
        /// <summary>
        /// 配置文件映射对象
        /// </summary>
        private static ExeConfigurationFileMap _map;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ConfigMonitor()
        {
            MonitorConfigFile();
            InitConnectionConfig();
        }

        /// <summary>
        /// 配置文件变更事件委托
        /// </summary>
        public delegate void EventHandlerAfterConfigModify();

        /// <summary>
        /// 配置文件变更事件
        /// </summary>
        public static event Action ConfigModifyInfoEvent;

        /// <summary>
        /// 配置对象
        /// </summary>
        public static Configuration ParamConfig { get; set; }

        /// <summary>
        /// 向订阅者发布信息
        /// </summary>
        private static void RaiseEvent()
        {
            ConfigModifyInfoEvent?.Invoke();
        }

        /// <summary>
        /// 初始化所有连接配置
        /// </summary>
        private static void InitConnectionConfig()
        {
            // 读取配置文件进行所有的连接初始化操作
            _map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };
            ParamConfig = ConfigurationManager.OpenMappedExeConfiguration(_map, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// 创建配置文件发动监听器
        /// </summary>
        private static void MonitorConfigFile()
        {
            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            fileWatcher.Filter = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile).Name;
            fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;

            // Add event handlers.
            fileWatcher.Changed += OnChanged;
            fileWatcher.Created += OnChanged;
            fileWatcher.Deleted += OnChanged;
            fileWatcher.Renamed += OnChanged;

            // Begin watching.
            fileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 配置文件变更事件处理器
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="e">事件参数</param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            //InitConnectionConfig();
            RaiseEvent();
        }
    }
}
