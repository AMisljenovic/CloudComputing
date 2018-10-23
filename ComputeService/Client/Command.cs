using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Command : ICommand
    {
        // storing instances of loaded assemblies and their class called instances
        static Dictionary<string, List<AssemblyInstance>> _assemblies = new Dictionary<string, List<AssemblyInstance>>();
        // storing assemblies that are loaded but not usable
        static List<string> _invalidAssemblies = new List<string>();
        // list for checking if we're trying to add assembly that's already loaded
        static Dictionary<string, List<string>> _otherContainerAssemblies = new Dictionary<string, List<string>>();
        

        // this method is called when we are starting failed container from this current one
        public KeyValuePair<int,List<string>> NewInstance(string directory, string endpoint, string processPath, List<string> stoppedDlls, List<string> loadedDlls)
        {
            if(!_otherContainerAssemblies.ContainsKey(directory))
            {
                _otherContainerAssemblies.Add(directory, new List<string>());
            }
            _otherContainerAssemblies[directory].AddRange(loadedDlls);
            var invalidFiles = ContainerPath(directory,stoppedDlls);
            var process = Process.Start(processPath, endpoint);
            return new KeyValuePair<int, List<string>>(process.Id, invalidFiles);
        }

        // method for loading assemblies and trying to create instace of their class and start it
        public List<string> ContainerPath(string container, List<string> stoppedDlls)
        {
            List<string> invalidFiles = new List<string>();
            var files = Directory.GetFiles(container);
            foreach (var item in files)
            {
               

                Assembly assembly = null;
                string assemblyName = string.Empty;
                var filePath = item.Split(new string[] { "\\" }, StringSplitOptions.None);
                var itemName = filePath.LastOrDefault().Split('.');
                //checking if this current file is from another container and if it's already loaded
                if (_otherContainerAssemblies.ContainsKey(container))
                {
                    if (_otherContainerAssemblies[container].Contains(itemName[0]))
                    {
                        continue;
                    }
                }
                // deleting file if it's not with .dll extension
                if (!filePath[filePath.Length - 1].Contains(".dll"))
                {
                    invalidFiles.Add(filePath[filePath.Length - 1]);
                    File.Delete(item);
                    continue;
                }



                var isLoaded = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == itemName[0]);
                // if assembly is already loaded taking his name otherwise loading him
                if (isLoaded == null)
                {
                    assemblyName = itemName[0];
                }
                else
                {
                    assembly = Assembly.LoadFile(item);
                    assemblyName = assembly.GetName().Name;
                }
               
                // if it's loaded and invalid skipping it
                if(_invalidAssemblies.Contains(assemblyName))
                {
                    continue;
                }
                // if it's loaded and stopped it will be started again, otherwise it will be skipped
                else if (_assemblies.ContainsKey(container))
                {
                    if (_assemblies[container].Count(x => x.Assemblie.GetName().Name == assemblyName && x.State == DllState.STOP && !stoppedDlls.Contains(x.Assemblie.GetName().Name)) != 0)
                    {
                        var assemblyInstance = _assemblies[container].Where(x => x.Assemblie.GetName().Name == assemblyName).FirstOrDefault();
                        var type = assemblyInstance.Assemblie.GetType(String.Format("{0}.Worker", assembly.GetName().Name));
                        var mi = type.GetMethod("Start", new Type[1] { typeof(string) });
                        mi.Invoke(assemblyInstance.ObjectInstance, new object[1] { container });
                        assemblyInstance.State = DllState.START;
                        continue;
                    }
                    else if (_assemblies[container].Count(x => x.Assemblie.GetName().Name == assemblyName) != 0)
                    {
                        continue;
                    }
                }

                // trying to start new instance of assembly
                try
                {
                    var type = assembly.GetType(String.Format("{0}.Worker", assemblyName));
                    var obj = assembly.CreateInstance(type.ToString());
                    if (obj != null)
                    {
                        var mi = type.GetMethod("Start", new Type[1] { typeof(string) });
                        mi.Invoke(obj, new object[1] { container });
                        if (!_assemblies.ContainsKey(container))
                        {
                            _assemblies.Add(container, new List<AssemblyInstance>() { new AssemblyInstance() { Assemblie = assembly, ObjectInstance = obj, State = DllState.START } });
                            continue;
                        }
                        _assemblies[container].Add(new AssemblyInstance() { Assemblie = assembly, ObjectInstance = obj, State = DllState.START });

                    }
                }
                catch (Exception)
                {
                    // if it's invalid it will be added to return list and static list 
                    invalidFiles.Add(assemblyName);
                    _invalidAssemblies.Add(assembly.GetName().Name);
                }

            }
            return invalidFiles;
        }
        
        // calling this method we check if there's assembly loaded and instanced here and if it is, it will be stopped
        public string StopInstance(string assemblyName, string containerId)
        {
            if(_assemblies.ContainsKey(containerId))
            {
                var assemblyInstance = _assemblies[containerId].Where(x => x.Assemblie.GetName().Name == assemblyName).FirstOrDefault();
                if (assemblyInstance != null)
                {
                    var type = assemblyInstance.Assemblie.GetType(String.Format("{0}.Worker", assemblyInstance.Assemblie.GetName().Name));
                    var mi = type.GetMethod("Stop");
                    mi.Invoke(assemblyInstance.ObjectInstance, new object[0]);
                    assemblyInstance.State = DllState.STOP;
                    return "Instance stopped container at: " + Server.GetIP();
                }
                return "Didn't find assembly at: " + Server.GetIP();

            }
            else
            {
                return "Didn't find specified directory at: "+Server.GetIP();
            }
        }

        // getting number of argument called active assemblies in this container
        public int ActiveDll(string assemblyName)
        {
            int result = 0;
            foreach (var item in _assemblies.Values)
            {
                result += item.Count(x => x.Assemblie.GetName().Name == assemblyName && x.State == DllState.START);
            }
            return result;
        }

        // getting information if assembly is loaded
        public static bool IsLoaded(string containerId,string assemblyName)
        {
            if(_assemblies.ContainsKey(containerId))
            {
                if(_assemblies[containerId].Count(x=> x.Assemblie.GetName().Name == assemblyName) != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
