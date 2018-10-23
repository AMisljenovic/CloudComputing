using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
    public class ComputeManagement : IComputeManagement
    {
        // calling this method we are getting number of active instances of specfic assemblies 
        // and with it starting new ones, closing some or all depending on count,
        // or leaving with same number
        public string Scale(string assemblyName, uint count)
        {
            string result = string.Empty;
            if(count>5)
            {
                return "Number of instances must be between 1 and 4";
            }
            else if(!Program.RepoContaintsAssembly(assemblyName))
            {
                return "There is not dll with such name in repository";
            }

            int counter = Program.GetActiveInstances(assemblyName);

            var difference = count - counter;

            if (difference == 0)
            {
                return "Number of instances remains same";
            }
            else if (difference > 0)
            {
                Program.Scale(assemblyName, count);
                return string.Format("{0} new instace(s) of {1} is activated", difference, assemblyName);
            }
            else
            {
                for (int i = counter-1; i >= count; i--)
                {
                    result += Program.StopInstance(assemblyName, i);
                }
                return result;
            }
        }
    }
}
