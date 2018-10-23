using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace CloudCompute
{

    class Program
    {
        static uint _changeInstance = 0;
        static string _assemblyName = string.Empty;
        static string[] _currentRepo;
        static object _obj = new object();
        static object _aiobj = new object();
        static readonly int containersNo = 4;
        static Configuration _configuration;
        static List<string> _repoDlls = new List<string>();
        static CancellationTokenSource _token = new CancellationTokenSource();
        static void Main(string[] args)
        {

            // writing on specific location then reading from it
            ReadWrite readWrite = new ReadWrite();
            readWrite.DataInput(readWrite);
            _configuration = readWrite.DeSerializeObject<Configuration>("ConfigFile.xml");

            // case if container number or dll directory is invalid
            if (Directory.Exists(_configuration.DllLocation))
            {
                if (_configuration.InstaceCount > 4)
                {
                    var files = Directory.GetFiles(_configuration.DllLocation);
                    foreach (var item in files)
                    {
                        File.SetAttributes(item, FileAttributes.Normal);
                        File.Delete(item);
                    }
                    throw new Exception("Number of instance must be in range 1 to 4");
                }
            }
            else
            {
                throw new Exception("Dll location not found!");
            }

            
            RoleServer server = new RoleServer();
            server.Open();
            Console.WriteLine("RoleEnviorment server is alive!");
            ComputeServer computeServer = new ComputeServer();
            computeServer.Open();
            Console.WriteLine("ComputeManagment server is alive!");

            // adding containers to array
            for (int i = 0; i < containersNo; i++)
            {
                Container.AddContainer(i, _configuration.Path);
            }
            // tasks that are checking predefined location and if is container alive
            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => LocationCheck());
            tasks[1] = Task.Run(() => HearthBeat());
            Console.Read();
            
            _token.Cancel();
            Task.WaitAll(tasks);
            // after waiting for all work to be finished killing all process and closing servers
            foreach (var item in Container.GetContainers())
            {
                if (!item.Process.HasExited)
                {
                    item.Process.Kill();
                }
            }

            server.Close();
            computeServer.Close();
        }

        // stopping assembly within specific container
        internal static string StopInstance(string assemblyName,int index)
        {
            try
            {
               Container.GetContainer(index).StoppedDlls.Add(assemblyName);
                return Container.GetContainer(index).Command.StopInstance(assemblyName, Container.GetContainer(index).Directory) + Environment.NewLine;
            }
            catch (CommunicationException e)
            {
                return e.Message + Environment.NewLine;
            }
        }

        // getting number of active assemblies within all containers
        internal static int  GetActiveInstances(string assemblyName)
        {
            int activeDll = 0;
            int contains = -1;
            foreach (var item in Container.GetContainers())
            {
                if (item.Directory != null)
                {
                    if (item.ContainerManagement.Load(item.Directory,assemblyName))
                    {
                        contains++;
                    }
                }
            }
            if (contains == -1)
            {
                return activeDll;
            }
            for (int i = 0; i < contains; i++)
            {
                activeDll += Container.GetContainer(i).Command.ActiveDll(assemblyName);
            }
            return activeDll;

        }
        //checking if theres assembly in repository
        internal static bool RepoContaintsAssembly(string assemblyName)
        {
            if (_repoDlls.Contains(assemblyName))
                return true;
            return false;
        }

        // sending assembly name that needs to be scaled across containers
        internal static void Scale(string assmeblyName,uint count)
        {
            lock (_aiobj)
            {
                _assemblyName = assmeblyName;
                _changeInstance = count;
                for (int i = 0; i < count; i++)
                {
                    if (Container.GetContainer(i).StoppedDlls.Contains(assmeblyName))
                    {
                        Container.GetContainer(i).StoppedDlls.Remove(assmeblyName);
                    }
                }
            }
        }

        //checking if container is alive
        static void HearthBeat()
        {
            while (true)
            {
                if(_token.IsCancellationRequested)
                {
                    return;
                }
                int index = 0;
                foreach (var item in Container.GetContainers().ToList())
                {
                    

                    try
                    {
                        if (item.ContainerState != State.INITIALIZING)
                        {
                            Console.WriteLine(item.ContainerManagement.CheckHealth());
                            if (item.ContainerState == State.FAIL)
                            {
                                item.RestoreCommand();
                                item.ContainerState = State.STANDBY;
                            }
                            lock (_obj)
                            {
                                if (item.ContainerState == State.STANDBY && item.Dlls.Count != 0)
                                {
                                    // checking if assembly is loaded
                                    foreach (var value in Container.GetContainer(index).Dlls.ToList())
                                    {                                       
                                        if (item.ContainerManagement.Load(item.Directory,value))
                                        {
                                            Console.WriteLine("{0} in container:{1} loaded", value, item.Endpoint);
                                            Container.GetContainer(index).LoadedDlls.Add(value);
                                        }
                                        else
                                        {
                                            Console.WriteLine("{0} in container:{1} didn't load", value, item.Endpoint);
                                        }
                                        Container.GetContainer(index).Dlls.Remove(value);

                                    }
                                }
                            }
                        }

                    }
                    // if container has failed restore is called
                    catch (Exception)
                    {
                        Container.GetContainer(index).ContainerState = State.FAIL;
                        Console.WriteLine("Container at:{0} is {1}.", Container.GetContainer(index).Endpoint, Container.GetContainer(index).ContainerState);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        RestoreContainer(index);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    }
                    index++;
                    Thread.Sleep(1000);
                }



            }
        }

        // periodically cheking for new files
        static void LocationCheck()
        {
            while (true)
            {
                if (_token.IsCancellationRequested)
                {
                    return;
                }
                uint instances = _configuration.InstaceCount;
                var files = Directory.GetFiles(_configuration.DllLocation);
                if (files.Length == 0)
                {
                    Console.WriteLine("There are no dlls in repository");
                    Thread.Sleep(2000);
                    continue;
                }
                if (_assemblyName == string.Empty)
                {
                    if (_currentRepo == files)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }
                }
                lock (_aiobj)
                {
                    _currentRepo = files;
                    foreach (var file in files)
                    {
                        var fileName = file.Split(new string[] { "\\" }, StringSplitOptions.None);
                        var dllName = fileName.LastOrDefault().Split('.');
                        if (!_repoDlls.Contains(dllName[0]))
                        {
                            _repoDlls.Add(dllName[0]);
                        }
                        if (_assemblyName != string.Empty)
                        {
                            if (dllName[0] != _assemblyName)
                            {
                                continue;
                            }
                        }                       
                        if (_changeInstance != 0)
                        {
                            instances = _changeInstance;
                        }
                        for (int i = 0; i < instances; i++)
                        {
                            var directory = String.Format(_configuration.InputLocation + @"container{0}\", i);

                            Container.GetContainer(i).Directory = directory;

                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);

                            }
                            if (!File.Exists(String.Format(directory + fileName.LastOrDefault(), i)))
                            {                               
                                File.Copy(file, String.Format(directory + fileName.LastOrDefault(), i),true);
                                File.SetAttributes(String.Format(directory + fileName.LastOrDefault(), i), FileAttributes.Normal);
                                Container.GetContainer(i).Dlls.Add(dllName[0]);
                            }
                        }

                    }
                    _changeInstance = 0;
                    _assemblyName = string.Empty;
                    for (int i = 0; i < instances; i++)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        StartAsync(Container.GetContainer(i).Command,Container.GetContainer(i).Directory, i,Container.GetContainer(i).StoppedDlls);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    Thread.Sleep(2000);
                }
            }
           
        }
        // asynchronous starting assemblies for every container
        static async Task StartAsync(ICommand command,string containerId,int instanceNo,List<string> stoppedDlls)
        {
            lock (_obj)
            {
                Container.GetContainer(instanceNo).ContainerState = State.HOT;
            }
                
                await Task.Run(() =>
                {
                    try
                    {
                        var invalidFiles = command.ContainerPath(containerId,stoppedDlls);
                        DeleteInvalidFiles(invalidFiles,instanceNo);
                        Container.GetContainer(instanceNo).ContainerState = State.STANDBY;
                    }
                    catch (CommunicationException e)
                    {
                        Container.GetContainer(instanceNo).ContainerState = State.FAIL;
                        Console.WriteLine("Communication with container:{0} is in {1} state beacuse: {2}", Container.GetContainer(instanceNo).Endpoint, Container.GetContainer(instanceNo).ContainerState, e.Message);
                    }

                });
        }


        // asynchronous restoring container from another one, otherwise from CloudCompute
        static async Task RestoreContainer(int indexOfFailed)
        {
            for (int i = 0; i < Container.GetContainers().Length; i++)
            {
                if (i != indexOfFailed && (Container.GetContainer(i).ContainerState == State.STANDBY || Container.GetContainer(i).ContainerState == State.STARTED))
                {
                    lock (_obj)
                    {
                        Container.GetContainer(i).ContainerState = State.HOT;
                        Container.GetContainer(indexOfFailed).ContainerState = State.INITIALIZING;
                    }                  
                    await Task.Run(() =>
                    {
                        try
                        {
                            var keyValuePair = Container.GetContainer(i).Command.NewInstance(Container.GetContainer(indexOfFailed).Directory, Container.GetContainer(indexOfFailed).Endpoint, Container.GetContainer(indexOfFailed).ProcessPath, Container.GetContainer(indexOfFailed).StoppedDlls, Container.GetContainer(indexOfFailed).LoadedDlls);
                            Container.GetContainer(indexOfFailed).Process = Process.GetProcessById(keyValuePair.Key);
                            Container.GetContainer(indexOfFailed).StartProcessAndConnectionAgain(keyValuePair.Key);
                            Console.WriteLine("Container at:{0} is {1}", Container.GetContainer(indexOfFailed).Endpoint, Container.GetContainer(indexOfFailed).ContainerState);
                            DeleteInvalidFiles(keyValuePair.Value, indexOfFailed);
                            Container.GetContainer(i).ContainerState = State.STANDBY;
                        }
                        catch (Exception e)
                        {
                            Container.GetContainer(i).ContainerState = State.FAIL;
                            Console.WriteLine("Communication with container:{0} is in {1} state beacuse: {2}", Container.GetContainer(i).Endpoint, Container.GetContainer(i).ContainerState, e.Message);
                            ResotreContainerFromCompute(indexOfFailed);
                        }
                    }
                    );
                    return;                    
                }
            }
            ResotreContainerFromCompute(indexOfFailed);
            

        }

        // deleting invalid files from repository
        static void DeleteInvalidFiles(List<string> invalidFiles,int instanceNo)
        {
            if (invalidFiles.Count != 0)
            {
                foreach (var item in invalidFiles)
                {
                    Container.GetContainer(instanceNo).Dlls.Remove(item);
                    Console.WriteLine("{0} is invalid file", item);
                    var files = Directory.GetFiles(_configuration.DllLocation);
                    foreach (var file in files)
                    {
                        if (file.Contains(item))
                        {
                            Console.WriteLine("{0} is deleted from repository", item);
                            File.Delete(file);
                        }
                        if (_repoDlls.Contains(item))
                        {
                            _repoDlls.Remove(item);
                        }
                    }
                }
            }
        }

        // restoring container from CloudCompute and executing dlls
        static void ResotreContainerFromCompute(int indexOfFailed)
        {
            Container.GetContainer(indexOfFailed).StartProcessAndConnectionAgain();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StartAsync(Container.GetContainer(indexOfFailed).Command, Container.GetContainer(indexOfFailed).Directory, indexOfFailed, Container.GetContainer(indexOfFailed).StoppedDlls);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
