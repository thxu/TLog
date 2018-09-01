using System;
using System.ServiceModel;

namespace Ser1.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(Service.Service)))
            {
                host.Opened += (sender, eventArgs) =>
                {
                    Console.Out.WriteLine("Ser1 has been started up");
                };
                host.Open();
                Console.ReadKey();
            }
        }
    }
}
