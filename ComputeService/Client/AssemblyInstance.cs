using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    // Custom class to store instance of assembly object and it's current state
    class AssemblyInstance
    {
        public Assembly Assemblie { get; set; }
        public object ObjectInstance { get; set; }
        public DllState State { get; set; }
    }
}
