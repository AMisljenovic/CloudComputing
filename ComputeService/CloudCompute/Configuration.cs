using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudCompute
{
    // this class is for writing and reading an xml file while starting PaaS
    [Serializable]
    class Configuration
    {      
        public  uint InstaceCount { get; set; }         
        public string Path { get; set; }      
        public string DllLocation{ get; set; }
        public string InputLocation { get; set; }
    }
}
