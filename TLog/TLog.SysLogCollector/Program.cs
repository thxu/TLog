using System;
using System.IO;
using System.Linq;

namespace TLog.SysLogCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            CollectorHandler handler = new CollectorHandler();
            handler.ThreadProc();
            handler.Start();

            string currVersion = AppDomain.CurrentDomain.BaseDirectory.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();
            Console.WriteLine("\r\n     系统日志归集调度程序已经启动，Version = " + currVersion);
            Console.WriteLine("\r\n     启动时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("\r\n     若需退出请输入 exit 按回车退出...\r\n");
            string userCommand = string.Empty;
            while (userCommand != "exit")
            {
                if (string.IsNullOrEmpty(userCommand) == false)
                    Console.WriteLine("     非退出指令,自动忽略...");
                userCommand = Console.ReadLine();
            }
        }
    }
}
