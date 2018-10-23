using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
    // starting WCF server with implemented IRoleEnvironment
    class RoleServer
    {
        string computeEndpoint = "net.tcp://localhost:10050/Service";
        ServiceHost _serviceHost = null;

        public RoleServer()
        {
            _serviceHost = new ServiceHost(typeof(RoleEnviorment));
            _serviceHost.AddServiceEndpoint(typeof(IRoleEnvironment), new NetTcpBinding(), computeEndpoint);
        }

        public void Open()
        {
            _serviceHost.Open();
        }

        public void Close()
        {
            _serviceHost.Close();
        }

    }
}
