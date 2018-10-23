using Contracts;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Client
{
    class JobContainer : IContainerManagement
    {
        // returning string that server is alive
        public string CheckHealth()
        {
            return String.Format("Container at: {0} is alive", Server.GetIP());
        }

        // returning information if is assembly loaded in this container by calling static method in Command
        public bool Load(string containerId,string assemblyName)
        {

            return Command.IsLoaded(containerId, assemblyName);         
        }       
    }
}