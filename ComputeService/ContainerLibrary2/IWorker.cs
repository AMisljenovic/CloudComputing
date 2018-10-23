using Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ContainerLibrary2
{
    [ServiceContract]
    public interface IWorker
    {
        [OperationContract]
        void Start(String containerId);

        [OperationContract]
        void Stop();
    }
}
