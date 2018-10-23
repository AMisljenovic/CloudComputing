using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
    // starting WCF server with implemented IComputeManagement
    public class ComputeServer
    {
        string computeEndpoint = "net.tcp://localhost:10050/Compute";
        ServiceHost _serviceHost = null;

        public ComputeServer()
        {
            _serviceHost = new ServiceHost(typeof(ComputeManagement));
            _serviceHost.AddServiceEndpoint(typeof(IComputeManagement), new NetTcpBinding(), computeEndpoint);
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
