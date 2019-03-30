using System;
using TLog.Core.AOP;
using TLog.Core.ContextPropagation;
using TLog.Core.Log;

namespace Ser1.Service
{
    public class Service : IContract.IContract
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ContextReceiveBehavior(IsReturnContext = false)]
        public int Add(int a, int b)
        {
            int res1 = new Logic().Add1(a, b);
            int res2 = new Logic().Add2(a, b);
            int res3 = Logic.AddStatic1(a, b);
            int res4 = Logic.AddStatic2(a, b);
            return res1 + res2 + res3 + res4;
        }
    }

    [RunningLog]
    public class Logic
    {
        public int Add1(int a, int b)
        {
            return a + b;
        }

        public int Add2(int a, int b)
        {
            if (a <= 0)
            {
                throw new Exception("a <= 0");
            }

            return a + b;
        }

        public static int AddStatic1(int a, int b)
        {
            return a + b;
        }

        public static int AddStatic2(int a, int b)
        {
            if (a <= 0)
            {
                throw new Exception("a <= 0 static");
            }
            return a + b;
        }
    }
}
