using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
        
    // class that is used to control containers
    public class Container
    {
        
        static Container[] _containers = new Container[4];
        public Process Process { get; set; }
        public string ProcessPath { get; set; }
        public string Directory { get; set; }
        public State ContainerState { get; set; }
        public List<string> Dlls { get; set; }
        public List<string> LoadedDlls { get; set; }
        public List<string> StoppedDlls { get; set; }
        public ICommand Command { get; set; }
        public IContainerManagement ContainerManagement  { get; set; }
        public string Endpoint { get; set; }
        

        public Container(string endpoint,string proccessPath)
        {
            Endpoint = endpoint;
            ProcessPath = proccessPath;
            StoppedDlls = new List<string>();
            LoadedDlls = new List<string>();
            Dlls = new List<string>();
            try
            {
                Process = Process = Process.Start(proccessPath, endpoint);
                ContainerState = State.STARTED;
                Command = new ChannelFactory<ICommand>(new NetTcpBinding(), endpoint + "Command").CreateChannel();
                ContainerManagement = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), endpoint + "Container").CreateChannel();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                ContainerState = State.FAIL;
            }
           

        }

        public void StartProcessAndConnectionAgain(int id = -1)
        {
            try
            {
                if(id == -1)
                {
                    Process = System.Diagnostics.Process.Start(ProcessPath, Endpoint);
                }
                var commandBinding = new NetTcpBinding();
                commandBinding.SendTimeout = new TimeSpan(0, 0, 10);
                var conBinding = new NetTcpBinding();
                conBinding.SendTimeout = new TimeSpan(0, 0, 10);
                Command = new ChannelFactory<ICommand>(commandBinding, Endpoint + "Command").CreateChannel();
                ContainerManagement = new ChannelFactory<IContainerManagement>(conBinding, Endpoint + "Container").CreateChannel();
                ContainerState = State.STANDBY;
            }
            catch (CommunicationException e)
            {
                ContainerState = State.FAIL;
                Console.WriteLine(e.Message);
            }
        }

        public void RestoreCommand()
        {
            Command = new ChannelFactory<ICommand>(new NetTcpBinding(), Endpoint + "Command").CreateChannel();
        }

        public static void AddContainer(int index,string containerPath)
        {
            if(index > 3)
            {
                Console.WriteLine("There can be only 4 containers");
                return;
            }
            _containers[index] = new Container(String.Format("net.tcp://localhost:10{0}/", 10 + (index * 10)), containerPath);

        }

        public static Container GetContainer(int index)
        {
            return _containers[index];
        }

        public static Container[] GetContainers()
        {
            return _containers;
        }

    }
}
