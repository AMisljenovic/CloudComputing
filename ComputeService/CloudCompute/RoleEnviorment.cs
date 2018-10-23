using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
    public class RoleEnviorment : IRoleEnvironment
    {
        object _obj = new object();
        object _obj1 = new object();
        static Dictionary<string, List<string>> _brotherInstances = new Dictionary<string, List<string>>();

        // getting address for calling assembly
        public string AcquireAddress(string myAssemblyName, string containerId)
        {
            lock (_obj)
            {
                var directoryName = containerId.Split(new string[] { "\\" }, StringSplitOptions.None);
                var address = String.Format("net.tcp://localhost:{0}/{1}", GetPort(), directoryName[directoryName.Length-2]);
                try
                {
                    _brotherInstances.Add(myAssemblyName, new List<string>() { address });
                }
                catch (Exception)
                {
                    _brotherInstances[myAssemblyName].Add(address);
                }
                
                return address;
            }

        }
        // getting brother instances for calling assembly
        public string[] BrotherInstances(string myAssemblyName)
        {

            lock (_obj1)
            {
                if (_brotherInstances.ContainsKey(myAssemblyName))
                {
                    return _brotherInstances[myAssemblyName].ToArray();
                }
                return new string[] { };
            }        
        }

        // method for getting free port
        private int GetPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
