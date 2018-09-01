namespace TLog.Core.Model
{
    /// <summary>
    /// 异常日志
    /// </summary>
    public class ExceptionLog : LogBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ExceptionLog()
        {
            LogLevel = LogLevel.Error;
        }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string ExceptionInfo { get; set; }
    }
}
