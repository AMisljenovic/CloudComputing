using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface ICommand
    {
        [OperationContract]
        List<string> ContainerPath(string container,List<string> stoppedDlls);
        [OperationContract]
        KeyValuePair<int, List<string>> NewInstance(string directory, string endpoint, string processPath, List<string> stoppedDlls, List<string> loadedDlls);
        [OperationContract]
        string StopInstance(string assemblyName,string containerId);
        [OperationContract]
        int ActiveDll(string assemblyName);

    }
}
