using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ContainerLibrary
{
    [ServiceContract]
    public interface ITest
    {
        [OperationContract]
        void TestMethod();
    }
}
