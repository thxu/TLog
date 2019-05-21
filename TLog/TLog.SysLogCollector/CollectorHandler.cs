using Nest;
using NetFrameWork.Common.Code;
using NetFrameWork.Common.Extension;
using NetFrameWork.Common.Write;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TLog.Core.Common;
using TLog.Core.Model;
using TLog.SysLogCollector.Model;

namespace TLog.SysLogCollector
{
    public class CollectorHandler : QueueBase
    {
        private static ElasticClient _client = Config.GetNestClient();

        /// <summary>轮循的线程函数</summary>
        public override void ThreadProc()
        {
            try
            {
                var traceId = "190329092502c00239014691416d841bc56d7d99ed48";
                var spendTime = 10;
                var functionName = "1";
                var startTime = DateTime.Now.AddDays(-100);
                var stopTime = DateTime.Now.AddHours(1);
                var exStr = "1";
                var paramIn = "1";
                var paramOut = "1";
                var appName = "1";
                var logLevel = 10;
                var ip = "1";
                var customerStr = "1";

                var mustclauses = new List<QueryContainer>();
                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.TraceId),
                        Query = traceId
                    });
                }

                if (spendTime > 0)
                {
                    mustclauses.Add(new NumericRangeQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.SpendTime),
                        LessThanOrEqualTo = 0,
                        GreaterThanOrEqualTo = spendTime,
                    });
                }

                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.FunctionName),
                        Query = functionName
                    });
                }

                mustclauses.Add(new DateRangeQuery()
                {
                    Field = Infer.Field<SysLog>(f => f.CreateTime),
                    LessThanOrEqualTo = stopTime,
                    GreaterThanOrEqualTo = startTime,
                });
                if (!string.IsNullOrWhiteSpace(exStr))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.ExceptionInfo),
                        Query = exStr
                    });
                }
                if (!string.IsNullOrWhiteSpace(paramIn))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.ParamIn),
                        Query = paramIn
                    });
                }
                if (!string.IsNullOrWhiteSpace(paramOut))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.ParamOut),
                        Query = paramOut
                    });
                }
                if (!string.IsNullOrWhiteSpace(appName))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.AppName),
                        Query = appName
                    });
                }
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    mustclauses.Add(new TermQuery()
                    {
                        Field = Infer.Field<SysLog>(f => f.Ip),
                        Value = ip
                    });
                }
                if (!string.IsNullOrWhiteSpace(customerStr))
                {
                    mustclauses.Add(new MatchQuery
                    {
                        Field = Infer.Field<SysLog>(f => f.CustomerInfo),
                        Query = customerStr
                    });
                }

                if (logLevel > 0)
                {
                    mustclauses.Add(new TermQuery()
                    {
                        Field = Infer.Field<SysLog>(f => f.LogLevel),
                        Value = logLevel,
                    });
                }

                var searchRequest = new SearchRequest<SysLog>(Nest.Indices.Index("systemlog"), Types.Type("syslog"))
                {
                    From = 0,
                    Size = 1000,
                    Query = new BoolQuery { Must = mustclauses }
                };

                var tmp = _client.Search<SysLog>(searchRequest).Documents;


                foreach (string logFolder in Config.Logs)
                {
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo(logFolder);
                        List<FileInfo> files = new List<FileInfo>();
                        GetAllFiles(dir, files);

                        // 日志文件解析并入库
                        foreach (FileInfo file in files)
                        {
                            SaveLog(file);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(e, "归集指定文件夹异常");
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(e, "日志归集异常");
            }
        }

        /// <summary>时间间隔</summary>
        public override double Interval => Config.Interval * 1000;

        /// <summary>
        /// 获取指定文件夹下所有需要解析的日志文件集合
        /// </summary>
        /// <param name="dir">要搜索的文件夹</param>
        /// <param name="files">日志集合</param>
        private void GetAllFiles(DirectoryInfo dir, List<FileInfo> files)
        {
            if (!dir.Exists)
            {
                throw new Exception($"目录：{dir.FullName}没有找到");
            }

            if (dir.GetDirectories().Length <= 0)
            {
                // 该文件夹下面没有文件夹了,说明已经到底了，现在开始计算需要解析的日志文件
                var logFiles = dir.GetFiles("*.*").Where(n => n.Name.StartsWith("RunningLog")
                                                            || n.Name.StartsWith("Info")
                                                            || n.Name.StartsWith("Warning")
                                                            || n.Name.StartsWith("Error")
                                                            || n.Name.StartsWith("Fatal")).ToList();
                if (logFiles.Count <= 0)
                {
                    // 没有任何日志文件，退出当前搜索
                    return;
                }

                // 找到最新的日志，判断创建时间是否在10分钟内，如果在，说明该日志文件正在被使用，跳过该文件
                var latestFile = logFiles.MaxElement(n => n.CreationTime);
                if (latestFile.CreationTime.AddMinutes(10) >= DateTime.Now)
                {
                    // 最新的日志，归集方式修改为：先保存当前解析的文件内容，若入库失败，则回写此内容到文件。
                    SaveLatestLog(latestFile);
                    logFiles.Remove(latestFile);
                }
                files.AddRange(logFiles);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                if (subDir.FullName.Contains("ExceptionLog")
                 || subDir.FullName.Contains("InnerExceptionLog"))
                {
                    continue;
                }
                GetAllFiles(subDir, files);
            }
        }

        private List<T> GetLogModelAndClearFile<T>(FileInfo file, out string content)
        {
            content = string.Empty;
            try
            {
                List<T> res = new List<T>();
                using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    try
                    {
                        var buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        content = Encoding.UTF8.GetString(buffer);
                        fileStream.SetLength(0);
                        fileStream.Write(new byte[0], 0, 0);
                        fileStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                    }
                    finally
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
                //content = File.ReadAllText(file.FullName);

                Regex reg = new Regex(@"--------------------------------Start---------------------------------------(?<Log>(\s|\S)*?)--------------------------------End-----------------------------------------");

                var matchRes = reg.Matches(content);
                foreach (Match match in matchRes)
                {
                    if (match.Success)
                    {
                        var tmp = match.Groups["Log"].ToString().TrimStart();
                        T log = tmp.DeserializeObject<T>();
                        res.Add(log);
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                LogService.WriteLog(e, "解析日志文件异常，文件：" + file.FullName);
                // 解析文件异常，回写内容到文件中
                WriteFileContent(file.FullName, content);
            }
            return null;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="fileFullName">文件路径</param>
        /// <param name="content">内容</param>
        private void WriteFileContent(string fileFullName, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }
            using (FileStream fileStream = new FileStream(fileFullName, FileMode.Append, FileAccess.Write, FileShare.Read))
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

        /// <summary>
        /// 保存最新的文件(和普通保存的区别是本方法不会删除文件，入库失败后会回写当前解析的内容到文件中)
        /// </summary>
        /// <param name="file">文件</param>
        private void SaveLatestLog(FileInfo file)
        {
            string content = string.Empty;
            try
            {
                List<SysLog> sysLogs;
                if (file.Name.StartsWith("Error"))
                {
                    var exlog = GetLogModelAndClearFile<ExceptionLog>(file, out content);
                    if (exlog == null || !exlog.Any())
                    {
                        return;
                    }

                    sysLogs = BuildSysLog(exlog);
                }
                else
                {
                    var log = GetLogModelAndClearFile<LogBase>(file, out content);
                    if (log == null || !log.Any())
                    {
                        return;
                    }
                    sysLogs = BuildSysLog(log);
                }

                BulkDescriptor descriptor = new BulkDescriptor();
                foreach (SysLog sysLog in sysLogs)
                {
                    descriptor.Index<SysLog>(op => op.Document(sysLog).Index("systemlog"));
                }

                var insertRes = _client.Bulk(descriptor);
                if (!insertRes.Errors)
                {
                    file.Delete();
                }
            }
            catch (Exception e)
            {
                WriteFileContent(file.FullName, content);
                LogService.WriteLog(e, "日志入库异常，信息：" + file.FullName);
            }
        }

        private List<T> GetLogModel<T>(FileInfo file)
        {
            try
            {
                List<T> res = new List<T>();
                string content = File.ReadAllText(file.FullName);
                if (string.IsNullOrWhiteSpace(content))
                {
                    file.Delete();
                    return null;
                }

                Regex reg = new Regex(@"--------------------------------Start---------------------------------------(?<Log>(\s|\S)*?)--------------------------------End-----------------------------------------");

                var matchRes = reg.Matches(content);
                foreach (Match match in matchRes)
                {
                    if (match.Success)
                    {
                        var tmp = match.Groups["Log"].ToString().TrimStart();
                        T log = tmp.DeserializeObject<T>();
                        res.Add(log);
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                LogService.WriteLog(e, "解析日志文件异常，文件：" + file.FullName);
            }
            return null;
        }

        /// <summary>
        /// 日志入库
        /// </summary>
        /// <param name="file">要解析并入库的日志文件</param>
        private void SaveLog(FileInfo file)
        {
            try
            {
                List<SysLog> sysLogs;
                if (file.Name.StartsWith("Error"))
                {
                    var exlog = GetLogModel<ExceptionLog>(file);
                    if (exlog == null || !exlog.Any())
                    {
                        return;
                    }

                    sysLogs = BuildSysLog(exlog);
                }
                else
                {
                    var log = GetLogModel<LogBase>(file);
                    if (log == null || !log.Any())
                    {
                        return;
                    }
                    sysLogs = BuildSysLog(log);
                }

                BulkDescriptor descriptor = new BulkDescriptor();
                foreach (SysLog sysLog in sysLogs)
                {
                    descriptor.Index<SysLog>(op => op.Document(sysLog).Index("systemlog"));
                }

                var insertRes = _client.Bulk(descriptor);
                if (!insertRes.Errors)
                {
                    file.Delete();
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(e, "日志入库异常，信息：" + file.FullName);
            }
        }

        private List<SysLog> BuildSysLog(List<ExceptionLog> logs)
        {
            List<SysLog> res = new List<SysLog>();
            foreach (ExceptionLog log in logs)
            {
                res.Add(new SysLog
                {
                    AppName = log.LogSpan.AppName,
                    CreateTime = log.LogSpan.CreateTime,
                    CustomerInfo = log.CustomerInfo.ToJson(),
                    ExceptionInfo = log.ExceptionInfo,
                    FunctionName = log.LogSpan.FunctionName,
                    //Id = 0,
                    Ip = log.LogSpan.Ip,
                    LogLevel = log.LogLevel.GetHashCode(),
                    ParamIn = log.LogSpan.ParamIn,
                    ParamOut = log.LogSpan.ParamOut,
                    SpanChain = log.LogSpan.SpanChain,
                    SpendTime = (decimal)log.LogSpan.SpendTime,
                    TraceId = log.LogSpan.TraceId,
                });
            }

            return res;
        }

        private List<SysLog> BuildSysLog(List<LogBase> logs)
        {
            List<SysLog> res = new List<SysLog>();
            foreach (LogBase log in logs)
            {
                res.Add(new SysLog
                {
                    AppName = log.LogSpan.AppName,
                    CreateTime = log.LogSpan.CreateTime,
                    CustomerInfo = log.CustomerInfo.ToJson(),
                    ExceptionInfo = string.Empty,
                    FunctionName = log.LogSpan.FunctionName,
                    //Id = 0,
                    Ip = log.LogSpan.Ip,
                    LogLevel = log.LogLevel.GetHashCode(),
                    ParamIn = log.LogSpan.ParamIn,
                    ParamOut = log.LogSpan.ParamOut,
                    SpanChain = log.LogSpan.SpanChain,
                    SpendTime = (decimal)log.LogSpan.SpendTime,
                    TraceId = log.LogSpan.TraceId,
                });
            }

            return res;
        }
    }
}
