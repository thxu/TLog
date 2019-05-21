using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TLog.Core.Common;
using TLog.Core.ContextPropagation;
using TLog.Core.Model;

namespace TLog.Core.Log
{
    /// <summary>
    /// 日志管理
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// 日志记录者实例
        /// </summary>
        private static ILog _logger;

        /// <summary>
        /// 随机数发生器
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// 静态构造器
        /// </summary>
        static LogManager()
        {
            IniLogger();
            ConfigMonitor.ConfigModifyInfoEvent += IniLogger;
        }

        private static void IniLogger()
        {
            try
            {
                Assembly impAssembly = Assembly.Load(LogConfig.LogProviderAssembly);
                var logProviderType = impAssembly.GetType(LogConfig.LogProviderType);
                _logger = (ILog)Activator.CreateInstance(logProviderType);
            }
            catch (Exception e)
            {
                _logger = new TxtLogger();
                InnerTxtLog.WriteException(e, "初始化日志提供者异常");
            }
        }

        /// <summary>
        /// 自定义追踪id（注意，此函数会打破原调用链并新建调用链， 建议在http调用的第一个函数或者调度程序的入口函数使用此方法）
        /// </summary>
        /// <param name="traceId"></param>
        public static void CustomerTraceId(string traceId = "")
        {
            LogContext.Current.TraceId = LogSpan.CreateNewTraceId(traceId);
            LogContext.Current.SpanChain = "0";
        }

        /// <summary>
        /// 获取当前调用链的追踪id
        /// </summary>
        /// <returns>追踪id</returns>
        private static string GetCurrTraceId()
        {
            return LogContext.Current.TraceId;
        }

        /// <summary>
        /// 记录内部异常记录
        /// </summary>
        /// <param name="e">异常信息</param>
        /// <param name="remark">备注信息</param>
        /// <param name="logSpan">日志段</param>
        internal static void InnerException(Exception e, string remark, LogSpan logSpan)
        {
            Task.Run(() =>
            {
                try
                {
                    ExceptionLog log = new ExceptionLog
                    {
                        CustomerInfo = new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        ExceptionInfo = e.ToString(),
                        LogLevel = LogLevel.Error,
                    };
                    log.CustomerInfo.Add("Remark", string.IsNullOrWhiteSpace(remark) ? string.Empty : remark);
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.WriteException(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录异常日志异常,参数：" + new { e, remark }.ToJson());
                }
            });
        }

        /// <summary>
        /// 记录内部运行日志
        /// </summary>
        internal static void InnerRunningLog(LogSpan logSpan)
        {
            Task.Run(() =>
            {
                try
                {
                    LogBase log = new LogBase
                    {
                        CustomerInfo = new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        LogLevel = LogLevel.RunningLog,
                    };
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.Write(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录运行日志异常,参数：" + logSpan.ToJson());
                }
            });
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="e">异常信息</param>
        /// <param name="remark">备注</param>
        /// <param name="args">请求参数</param>
        /// <param name="returnVal">返回参数</param>
        /// <param name="keyWord">自定义信息</param>
        public static void Exception(Exception e, string remark, object args = null, object returnVal = null, Dictionary<string, object> keyWord = null)
        {
            // 调用链+1
            LogSpan logSpan = LogSpan.Extend(LogContext.Current);
            var functionName = GetStackTrace();
            Task.Run(() =>
            {
                try
                {
                    ExceptionLog log = new ExceptionLog
                    {
                        CustomerInfo = keyWord ?? new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        ExceptionInfo = e.ToString(),
                        LogLevel = LogLevel.Error,
                    };
                    log.LogSpan.FunctionName = functionName;
                    log.LogSpan.ParamIn = args?.ToJson() ?? string.Empty;
                    log.LogSpan.ParamOut = returnVal?.ToJson() ?? string.Empty;
                    log.CustomerInfo.Add("Remark", string.IsNullOrWhiteSpace(remark) ? string.Empty : remark);
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.WriteException(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录异常日志异常,参数：" + new { e, remark, args, returnVal, keyWord }.ToJson());
                }
            });
        }

        /// <summary>
        /// 记录info信息
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="args">请求参数</param>
        /// <param name="returnVal">返回参数</param>
        /// <param name="keyWord">自定义信息</param>
        public static void Info(string content = "", object args = null, object returnVal = null, Dictionary<string, object> keyWord = null)
        {
            // 调用链+1
            LogSpan logSpan = LogSpan.Extend(LogContext.Current);
            var functionName = GetStackTrace();
            Task.Run(() =>
            {
                try
                {
                    LogBase log = new LogBase
                    {
                        CustomerInfo = keyWord ?? new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        LogLevel = LogLevel.Info,
                    };
                    log.LogSpan.FunctionName = functionName;
                    log.LogSpan.ParamIn = args?.ToJson() ?? string.Empty;
                    log.LogSpan.ParamOut = returnVal?.ToJson() ?? string.Empty;
                    log.CustomerInfo.Add("Content", string.IsNullOrWhiteSpace(content) ? string.Empty : content);
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.Write(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录Info日志异常,参数：" + new { content, args, returnVal, keyWord }.ToJson());
                }
            });
        }

        /// <summary>
        /// 记录info信息
        /// </summary>
        /// <param name="logSpan">日志段</param>
        /// <param name="paramOut">内容</param>
        internal static void InnerInfo(LogSpan logSpan, string paramOut = "")
        {
            Task.Run(() =>
            {
                try
                {
                    LogBase log = new LogBase
                    {
                        CustomerInfo = new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        LogLevel = LogLevel.Info,
                    };
                    log.LogSpan.ParamOut = paramOut;
                    log.CustomerInfo.Add("Content", logSpan.FunctionName ?? string.Empty);
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.Write(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录内部Info日志异常,参数：" + new { paramOut, logSpan }.ToJson());
                }
            });
        }

        /// <summary>
        /// 记录debug日志
        /// </summary>
        /// <param name="debugInfo">debug信息</param>
        /// <param name="args">请求参数</param>
        /// <param name="returnVal">返回参数</param>
        /// <param name="keyWord">自定义信息</param>
        public static void Debug(string debugInfo, object args = null, object returnVal = null, Dictionary<string, object> keyWord = null)
        {
            // 调用链+1
            LogSpan logSpan = LogSpan.Extend(LogContext.Current);
            var functionName = GetStackTrace();
            Task.Run(() =>
            {
                try
                {
                    LogBase log = new LogBase
                    {
                        CustomerInfo = keyWord ?? new Dictionary<string, object>(),
                        LogSpan = logSpan,
                        LogLevel = LogLevel.Warning,
                    };
                    log.LogSpan.FunctionName = functionName;
                    log.LogSpan.ParamIn = args?.ToJson() ?? string.Empty;
                    log.LogSpan.ParamOut = returnVal?.ToJson() ?? string.Empty;
                    log.CustomerInfo.Add("DebugInfo", string.IsNullOrWhiteSpace(debugInfo) ? string.Empty : debugInfo);
                    if (!LogFilter(log.LogLevel))
                    {
                        _logger.Write(log);
                    }
                }
                catch (Exception ex)
                {
                    InnerTxtLog.WriteException(ex, "记录Debug日志异常,参数：" + new { debugInfo, args, returnVal, keyWord }.ToJson());
                }
            });
        }

        /// <summary>
        /// 日志过滤器
        /// </summary>
        /// <param name="logLevel">当前日志等级</param>
        /// <returns>true=不记录日志</returns>
        private static bool LogFilter(LogLevel logLevel)
        {
            if (LogConfig.LogLevel == 0 || (LogConfig.LogLevel & logLevel.GetHashCode()) == logLevel.GetHashCode())
            {
                if (_random.Next(1, 100) <= (LogConfig.DropRate * 100))
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取函数调用堆栈
        /// </summary>
        /// <returns>调用堆栈</returns>
        private static string GetStackTrace()
        {
            StringBuilder res = new StringBuilder();
            StackTrace st = new StackTrace(true);
            var sf = st.GetFrames();
            if (sf != null)
            {
                foreach (StackFrame stackFrame in sf)
                {
                    var fileName = stackFrame.GetFileName();
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        continue;
                    }

                    var fullName = $"{stackFrame.GetMethod()?.DeclaringType?.FullName}.{stackFrame.GetMethod().Name}";
                    if (fullName.Contains("Go.Log.Core.Log.LogManager"))
                    {
                        continue;
                    }
                    res.Append($"{fullName} 行数={stackFrame.GetFileLineNumber()}" + Environment.NewLine);
                }
            }

            return res.ToString();
        }

        /// <summary>
        /// 隐藏原来的equals方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected  bool Equals(object obj)
        {
            return false;
        }
    }
}
