using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using TLog.Core.Common;
using TLog.Core.Model;

namespace TLog.Core.Log
{
    /// <summary>
    /// 文本日志纪录器
    /// </summary>
    internal class TxtLogger : ILog
    {
        /// <summary>
        /// 缓存写文本相关信息
        /// </summary>
        private static Dictionary<LogLevel, TextCacheModel> _cacheDic = new Dictionary<LogLevel, TextCacheModel>();

        /// <summary>
        /// 文件大小限制，单位M
        /// </summary>
        private static long fileMaxSize = 5;

        static TxtLogger()
        {
            _cacheDic.Add(LogLevel.RunningLog, new TextCacheModel());
            _cacheDic.Add(LogLevel.Info, new TextCacheModel());
            _cacheDic.Add(LogLevel.Warning, new TextCacheModel());
            _cacheDic.Add(LogLevel.Error, new TextCacheModel());
            _cacheDic.Add(LogLevel.Fatal, new TextCacheModel());
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
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            }
            return logs;
        }

        /// <summary>
        /// 写文本
        /// </summary>
        /// <param name="logLevel">日志类型</param>
        /// <param name="content">写入内容</param>
        /// <returns>写入的文件名</returns>
        private void WriteText(LogLevel logLevel, string content)
        {
            // 1、按顺序写入.1 .2文件
            // 2、每次写入前判断下文件大小，超过设定值则进行备份
            // 3、加锁保证并发写
            // 4、时间判断不做这么复杂了，用DateTime.Now
            DateTime time = DateTime.Now;
            string path = Path.Combine(GetLogPath(), logLevel.ToString(), $"{time:yyyyMMdd}");

            // 获取对应日志的缓存数据
            TextCacheModel cacheModel = _cacheDic[logLevel];

            string baseName = logLevel.ToString();

            // 最终操作的文件名
            string finalFileName = string.Empty;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // 文件名时间
            DateTime fileTime = time.Date.AddHours(time.Hour).AddMinutes(time.Minute).AddMinutes(-(time.Minute % 10));

            // 是否当前时间的日志
            bool iscurrentTime = true;

            if (fileTime > cacheModel.FileTime)
            {
                // 到了下一个10分钟,重置时间和索引
                cacheModel.FileTime = fileTime;
                cacheModel.FileIndex = 0;
            }
            else if (fileTime < cacheModel.FileTime)
            {
                // 说明是上一个10分钟的写入请求,查找一下上一个10分钟到哪个文件了
                // 理论上10分钟不可能写到1000个文件
                string existFile = string.Empty;
                for (int i = 0; i < 1000; i++)
                {
                    string foreFileFullName = $"{path}/{baseName}{fileTime}:MMddHHmm.log.{i}";
                    if (File.Exists(foreFileFullName))
                    {
                        existFile = foreFileFullName;
                    }
                    else
                    {
                        // 没有了，上次找到的是正确的
                        break;
                    }
                }

                // 就写这个文件了，剩下的日志也不会超过多少。
                finalFileName = existFile;
                iscurrentTime = false;
            }

            if (iscurrentTime)
            {
                // 序号处理
                if (cacheModel.FileIndex == -1)
                {
                    // 初始状态，需要查找一下当前的文件已经到了多少序号
                    int tempIndex = 0;
                    for (int i = 0; i < 1000; i++)
                    {
                        string foreFileFullName = $"{path}/{baseName}{fileTime}:MMddHHmm.log.{i}";
                        if (File.Exists(foreFileFullName))
                        {
                            tempIndex = i;
                        }
                        else
                        {
                            // 没有了，上次找到的是正确的
                            break;
                        }
                    }

                    cacheModel.FileIndex = tempIndex;
                }

                finalFileName = $"{path}/{baseName}{fileTime:MMddHHmm}.log.{cacheModel.FileIndex}";

                // 判断此文件是否已经写够容量
                FileInfo file = new FileInfo(finalFileName);
                if (file.Exists && file.Length > fileMaxSize * 1024 * 1024)
                {
                    // 超过大小，新建一个
                    finalFileName = $"{path}/{baseName}{time.ToString("MMddHHmm").Substring(0, 7) + "0"}.log.{cacheModel.FileIndex + 1}";
                    cacheModel.FileIndex++;
                }
            }
            else
            {
                // 这里就不处理序号什么的了
            }

            try
            {
                lock (cacheModel.SyncRoot)
                {
                    using (FileStream fileStream = new FileStream(finalFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        try
                        {
                            byte[] logs = Encoding.UTF8.GetBytes(content);
                            fileStream.Write(logs, 0, logs.Length);
                            fileStream.Flush();
                        }
                        finally
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                        }
                    }
                }
            }
            catch (IOException)
            {
                // 估计是和归集程序冲突了，过1s重试一次，再不行就算了
                Thread.Sleep(TimeSpan.FromSeconds(1));
                lock (cacheModel.SyncRoot)
                {
                    using (FileStream fileStream = new FileStream(finalFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        try
                        {
                            byte[] logs = Encoding.UTF8.GetBytes(content);
                            fileStream.Write(logs, 0, logs.Length);
                            fileStream.Flush();
                        }
                        finally
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 写入基础日志
        /// </summary>
        /// <param name="log">日志信息</param>
        public void Write(LogBase log)
        {
            StringBuilder content = new StringBuilder();
            content.Append("/**************************************************************************/" + Environment.NewLine);
            content.Append(DateTime.Now.ToString("日志时间:yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
            content.Append("Level:" + log.LogLevel + Environment.NewLine);
            content.Append(log.ToJson());
            content.Append(Environment.NewLine);
            content.Append(Environment.NewLine);
            WriteText(log.LogLevel, content.ToString());
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="log">日志信息</param>
        public void WriteException(ExceptionLog log)
        {
            StringBuilder content = new StringBuilder();
            content.Append("/**************************************************************************/" + Environment.NewLine);
            content.Append(DateTime.Now.ToString("日志时间:yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
            content.Append("Level:" + log.LogLevel + Environment.NewLine);
            content.Append(log.ToJson());
            content.Append(Environment.NewLine);
            content.Append(Environment.NewLine);
            WriteText(log.LogLevel, content.ToString());
        }
    }

    /// <summary>
    /// 文本日志缓存对象
    /// </summary>
    internal class TextCacheModel
    {
        /// <summary>
        /// 同步对象
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// 当前文本时间
        /// </summary>
        public DateTime FileTime { get; set; }

        /// <summary>
        /// 当前文本索引
        /// </summary>
        public int FileIndex { get; set; }

        /// <summary>
        /// 同步对象
        /// </summary>
        public object SyncRoot { get { return _syncRoot; } }
    }
}
