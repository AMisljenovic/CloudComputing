using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFClient
{
    // WCF client that controls instances of loaded assemblies
    class Program
    {
        static void Main(string[] args)
        {
            string computeEndpoint = "net.tcp://localhost:10050/Compute";
            IComputeManagement proxy;
            ChannelFactory<IComputeManagement> factory = new ChannelFactory<IComputeManagement>(new NetTcpBinding(), computeEndpoint);
            Console.WriteLine("To close program enter for assemblyName exit!");
            while(true)
            {
                try
                {
                    proxy = factory.CreateChannel();
                    Console.Write("Enter assemblyName: ");
                    string assemblyName = Console.ReadLine();
                    if (assemblyName == "exit")
                    {
                        break;
                    }
                    Console.Write("Enter instance number: ");
                    uint instanceNo = uint.Parse(Console.ReadLine());
                    
                    Console.WriteLine(proxy.Scale(assemblyName, instanceNo));
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
            }



        }
    }
}
