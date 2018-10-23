using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Role
{
    // dll that will be distributed across containers
    public class RoleEnvironment
    {
        static IRoleEnvironment _role;
        static string _computeEndpoint = "net.tcp://localhost:10050/Service";
        /// <summary>
        /// Vrednost je vrednost porta na kojoj se WCF server izvrsava
        /// Napomena: zbog jednostavnosti zadatka, moze biti samo jedan WCF server po klijentskom projektu
        /// </summary>
        public static String CurrentRoleInstance(String myAssembly, String containerId)
        {
            try
            {
                _role = new ChannelFactory<IRoleEnvironment>(new NetTcpBinding(), _computeEndpoint).CreateChannel();
                
                var address = _role.AcquireAddress(myAssembly, containerId);
                var assemblyInstances = _role.BrotherInstances(myAssembly);
                return address;

            }
            catch (CommunicationException e)
            {

                Console.WriteLine(e.Message);
                return string.Empty;
            }

        }
        /// <summary>
        /// Povratna vrednost je niz portova bratskih instanci.
        /// </summary>
        public static String[] BrotherInstances(string assemblyName)
        {
            try
            {
                _role = new ChannelFactory<IRoleEnvironment>(new NetTcpBinding(), _computeEndpoint).CreateChannel();
                return _role.BrotherInstances(assemblyName);
            }
            catch (Exception)
            {

                return new string[] { };
            }

        }

        
    }
}
