using System;
using System.IO;
using NetFrameWork.WCF;

namespace Ser1.WcfHelper
{
    public class Ser1Helper
    {
        private static readonly IContract.IContract _client;

        static Ser1Helper()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WCFConfig\\Ser1.config");
            _client = ServiceProxyFactory.Create<IContract.IContract>(configPath, "Ser1Endpoint");
        }

        public static int Add(int a, int b)
        {
            try
            {
                return _client.Add(a, b);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

            return -1;
        }
    }
}
