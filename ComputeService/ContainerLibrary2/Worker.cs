using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Role;

namespace ContainerLibrary2
{
    // testing dll with implemented IWorker
    public class Worker : IWorker
    {
        private ServiceHost _serviceHost = null;
        string _address = string.Empty;
        string _containerId = string.Empty;
        Thread thread;

        public Worker()
        {
                 
        }
       
        public void Start(string containerId)
        {
            _serviceHost = new ServiceHost(typeof(Test));
            try
            {
                if (_address == string.Empty)
                {
                    _containerId = containerId;
                    _address = RoleEnvironment.CurrentRoleInstance(Assembly.GetExecutingAssembly().GetName().Name, containerId);
                    
                }
                _serviceHost.AddServiceEndpoint(typeof(ITest), new NetTcpBinding(), _address);
                Console.WriteLine("Instance is alive at adress:{0}", _address);
                _serviceHost.Open();
                thread = new Thread(BrotherInstances);
                thread.Start();
            }
            catch (CommunicationException e)
            {

                Console.WriteLine(e.Message);
            }


        }

        public void Stop()
        {
            thread.Abort();
            _serviceHost.Close();
        }

        //periodically getting brother instances
        private void BrotherInstances()
        {
            while (true)
            {
                try
                {
                    
                    var brotherInstances = RoleEnvironment.BrotherInstances(Assembly.GetExecutingAssembly().GetName().Name).Where(x=> x != _address);
                    Console.WriteLine("For instance:{0}----------------------- brother instances: ",_address);
                    foreach (var item in brotherInstances)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine("------------------------------------------------- ");
                }
                catch (CommunicationException e)
                {

                    Console.WriteLine(e.Message);
                    break;
                }
                Thread.Sleep(2500);
                
            }
        }
    }
}
