using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // starting server with sent argument with endpoint or closing it due exception
            Server server = null;
            try
            {
                server = new Server(args[0]);
                server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("Container is running at:{0}", Server.GetIP());
            Console.Read();
            server.Stop();
        }
    }
}
