using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IRoleEnvironment
    {
        [OperationContract]
        String AcquireAddress(String myAssemblyName, String containerId);
        [OperationContract]
        String[] BrotherInstances(String myAssemblyName);
    }
}
