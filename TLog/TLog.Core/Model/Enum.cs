﻿namespace TLog.Core.Model
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 记录全部日志
        /// </summary>
        All = 0,

        /// <summary>
        /// (仅记录运行日志):函数调用、wcf接口调用日志等组件自动记录的日志
        /// </summary>
        RunningLog = 1,

        /// <summary>
        /// （仅记录一般信息）：记录系统运行中应该让用户知道的基本信息。
        /// 例如，服务开始运行，功能已经开户，编码已取消，无法导入编码等。
        /// </summary>    
        Info = 2,

        /// <summary>
        /// （仅记录警告）：记录系统中不影响系统继续运行，用户可正常完成操作
        /// 但不符合系统运行正常条件，有可能引起系统错误的信息。
        /// 例如，记录内容为空，数据内容不正确等。
        /// </summary>        
        Warning = 4,

        /// <summary>
        /// （仅记录一般错误）：记录系统中出现的导致系统不稳定，
        /// 部分功能出现混乱或部分功能失效一类的错误。
        /// 例如，数据字段为空，数据操作不可完成，操作出现异常等。
        /// </summary>     
        Error = 8,

        /// <summary>
        /// (仅记录致命错误)：记录系统中出现的能使用系统完全失去功能，服务停止，
        /// 系统崩溃等使系统无法继续运行下去的错误。
        /// 例如，数据库无法连接，系统出现死循环。
        /// </summary>        
        Fatal = 16,
    }
}
