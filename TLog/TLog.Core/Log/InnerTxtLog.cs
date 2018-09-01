using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TLog.Core.Log
{
    /// <summary>
    /// 内部文本日志，主要记录组件内部的错误日志
    /// </summary>
    public class InnerTxtLog
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object _syncObj = new object();

        /// <summary>
        /// 记录异常
        /// </summary>
        /// <param name="e">异常信息</param>
        /// <param name="remark">异常备注</param>
        public static void WriteException(Exception e, string remark)
        {
            Task.Run((() =>
            {
                lock (_syncObj)
                {
                    try
                    {
                        string content = DateTime.Now.ToString("日志时间:yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                                         CreateErrorMessage(e, remark) + Environment.NewLine;

                        DateTime timeStamp = DateTime.Now;
                        string path = GetFileMainPath(timeStamp);

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string fileName = Path.Combine(path, timeStamp.ToString("HH") + ".log");

                        FileStream fs = File.Open(fileName, FileMode.Append, FileAccess.Write);

                        try
                        {
                            byte[] logs = Encoding.UTF8.GetBytes(content);
                            fs.Write(logs, 0, logs.Length);
                            fs.Flush();
                        }
                        finally
                        {
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }));
        }

        /// <summary>
        /// 创建异常消息
        /// </summary>
        /// <param name="ex">异常信息</param>
        /// <param name="remark">备注</param>
        /// <returns>结果</returns>
        public static StringBuilder CreateErrorMessage(Exception ex, string remark)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("************************Exception Start********************************");
            string newLine = Environment.NewLine;
            stringBuilder.Append(newLine);
            stringBuilder.AppendLine("Exception Remark：" + remark);
            Exception innerException = ex.InnerException;
            stringBuilder.AppendFormat("Exception Date:{0}{1}", DateTime.Now, Environment.NewLine);
            if (innerException != null)
            {
                stringBuilder.AppendFormat("Inner Exception Type:{0}{1}", innerException.GetType(), newLine);
                stringBuilder.AppendFormat("Inner Exception Message:{0}{1}", innerException.Message, newLine);
                stringBuilder.AppendFormat("Inner Exception Source:{0}{1}", innerException.Source, newLine);
                stringBuilder.AppendFormat("Inner Exception StackTrace:{0}{1}", innerException.StackTrace, newLine);
            }
            stringBuilder.AppendFormat("Exception Type:{0}{1}", ex.GetType(), newLine);
            stringBuilder.AppendFormat("Exception Message:{0}{1}", ex.Message, newLine);
            stringBuilder.AppendFormat("Exception Source:{0}{1}", ex.Source, newLine);
            stringBuilder.AppendFormat("Exception StackTrace:{0}{1}", ex.StackTrace, newLine);
            stringBuilder.Append("************************Exception End************************************");
            stringBuilder.Append(newLine);
            return stringBuilder;
        }

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="timeStamp">timeStamp</param>
        /// <returns>path</returns>
        private static string GetFileMainPath(DateTime timeStamp)
        {
            return Path.Combine(GetLogPath(), "InnerExceptionLog", timeStamp.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// 获取日志路径
        /// </summary>
        /// <returns>路径</returns>
        private static string GetLogPath()
        {
            var logs = ConfigurationManager.AppSettings["Logs"];
            if (string.IsNullOrWhiteSpace(logs))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            return logs;
        }
    }
}
