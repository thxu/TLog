using System.ServiceModel;

namespace Ser1.IContract
{
    [ServiceContract]
    public interface IContract
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [OperationContract]
        int Add(int a, int b);
    }
}
