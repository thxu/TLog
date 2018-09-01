using TLog.Core.Model;

namespace TLog.Core.Log
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// 写入基础日志
        /// </summary>
        /// <param name="log">日志信息</param>
        void Write(LogBase log);

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="log">日志信息</param>
        void WriteException(ExceptionLog log);
    }
}
