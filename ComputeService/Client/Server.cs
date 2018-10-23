using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    // class for starting wcf servers with implemented ICommand and IContainerManagment
    class Server
    {
        ServiceHost _commandHost = null;
        ServiceHost _ContainerHost = null;
        private static string endpointAdress;
        public Server(string endpoint)
        {
            _commandHost = new ServiceHost(typeof(Command));
            _ContainerHost = new ServiceHost(typeof(JobContainer));
            var workerBinding = new NetTcpBinding();
            var containerBinding = new NetTcpBinding();

            _commandHost.AddServiceEndpoint(typeof(ICommand), workerBinding, endpoint + "Command");
            _ContainerHost.AddServiceEndpoint(typeof(IContainerManagement), containerBinding, endpoint + "Container");
            endpointAdress = endpoint;
        }

        public void Start()
        {

            _commandHost.Open();
            _ContainerHost.Open();
        }

        public void Stop()
        {
            _commandHost.Close();
            _ContainerHost.Close();
        }

        public static string GetIP()
        {
            return endpointAdress;
        }
    }
}
