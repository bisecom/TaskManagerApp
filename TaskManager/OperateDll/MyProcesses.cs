using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.TabsModels;
using System.Management;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace TaskManager.OperateDll
{
    public static class MyProcesses
    {
        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        private static List<ProcessModel> prosCollection;
        private static List<ServicesModel> servCollection;
        private static Process[] existingProcessList;
        static object lockerProcess;
        
        static MyProcesses()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            existingProcessList = Process.GetProcesses();
            lockerProcess = new object();
        }



        public static string getCurrentCpuUsage()
        {
            int countData = Convert.ToInt32(cpuCounter.NextValue());
            return countData.ToString() + "%";
        }

        public static string getCurrentProcessQty()
        {
            Process[] processList = Process.GetProcesses();
            return processList.Length.ToString();
        }

        public static string getAvailableRAM()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            int percent = 0;
            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                percent = Convert.ToInt32(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100);
            }

            return percent.ToString() + "%";

        }

        //			public static int getCurrMemory()
        //			{
        //				var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        //		
        //				var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
        //				    FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
        //				    TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
        //				}).FirstOrDefault();
        //				
        //				if (memoryValues != null) 
        //				{
        //				    int percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
        //				}	
        //			}

        public  static List<ProcessModel> GetProcesses()
        {
            prosCollection = new List<ProcessModel>();
            Process[] processList = Process.GetProcesses();
            int counter = 0;
            string PrName_ = "";

            lock (lockerProcess) // Thread safe code
            {
                foreach (Process process in processList)
                {
                    if (counter < 20)
                    {
                        ProcessModel temp = new ProcessModel();
                        //string status = (process.Responding == true ? "Работает" : "Не работает");
                        try
                        {
                            temp.ProcessIcon_ = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                        }
                        catch (Exception ex)
                        { };

                        try
                        { PrName_ = process.MainModule.FileName; }
                        catch (Exception ex) { }
                        if (PrName_ != "") temp.ProcessName_ = Path.GetFileName(PrName_);
                        else PrName_ = "";

                        dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                        temp.ProcessUsername_ = extraProcessInfo.Username;

                        //var cpu = new PerformanceCounter("Process", "% Processor Time", process.Id.ToString(), true);
                        //temp.ProcessLoad_ = cpu.ToString();
                        //dynamic result = new ExpandoObject();
                        //result.CPU = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);
                        //temp.ProcessLoad_ = result.CPU.ToString();
                        temp.ProcessMemory_ = BytesToReadableValue(process.PrivateMemorySize64);
                        temp.ProcessDescription_ = extraProcessInfo.Description;
                        temp.ProcessId = process.Id.ToString();
                        //temp.ServicesName_ = GetServiceName(process.Id);
                        prosCollection.Add(temp);
                        counter++;
                    }
                }
            }
            return prosCollection;
        }
     

        //--------------------------------------------------------------------------------
        public static ExpandoObject GetProcessExtraInformation(int processId)
        {
            // Query the Win32_Process
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            // Create a dynamic object to store some properties on it
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "Unknown";

            foreach (ManagementObject obj in processList)
            {
                // Retrieve username 
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return Username
                    response.Username = argList[0];

                    // You can return the domain too like (PCDesktop-123123\Username using instead
                    //response.Username = argList[1] + "\\" + argList[0];
                }

                // Retrieve process description if exists
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }

            return response;
        }
        //--------------------------------------------------------------------------------
        public static string BytesToReadableValue(long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }

        //--------------------------------------------------------------------------------
        public static String GetServiceName(int processId)
        {
            // Calling System.ServiceProcess.ServiceBase::ServiceNamea allways returns
            // an empty string,
            // see https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=387024

            // So we have to do some more work to find out our service name, this only works if
            // the process contains a single service, if there are more than one services hosted
            // in the process you will have to do something else
            try
            {
                String query = "SELECT * FROM Win32_Service where ProcessId = " + processId;
                System.Management.ManagementObjectSearcher searcher =
                    new System.Management.ManagementObjectSearcher(query);

                foreach (System.Management.ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["Name"].ToString();
                }
            }
            catch (Exception ex) {  }
            return "";
            //throw new Exception("Can not get the ServiceName");
        }


        //--------------------------------------------------------------------------------
        //Services
        public static List<ServicesModel> GetServices()
        {
            servCollection = new List<ServicesModel>();
            ServiceController[] scServices = ServiceController.GetServices();
            int counter = 0;
            //lock (servCollection) // Thread safe code
            //{

            var thread = new Thread(
              () =>
              {
                  foreach (ServiceController scTemp in scServices)
                {
                    ManagementObject service = new ManagementObject(@"Win32_service.Name='" + scTemp.ServiceName + "'");
                    object o = service.GetPropertyValue("ProcessId");

                    if (servCollection.FindIndex(f => f.ProcessId == ((UInt32)o).ToString()) < 0 && scTemp.Status == ServiceControllerStatus.Running && counter < 15)
                    {
                        ServicesModel temp = new ServicesModel();
                        temp.ServicesName = GetServiceName((int)((UInt32)o));
                        temp.ProcessId = ((UInt32)o).ToString();

                        temp.ServiceState = "Работает";//since it is filtered
                        try
                        {
                            using (ManagementObject service1 = new ManagementObject(new ManagementPath(string.Format("Win32_Service.Name='{0}'", temp.ServicesName))))
                            {
                                temp.ServiceDescription = service1["Description"].ToString();
                            }
                        }
                        catch (Exception ex) { }
                        temp.ServiceGroup = scTemp.ServicesDependedOn.ToString();

                        servCollection.Add(temp);
                    }
                    counter++;
                }
        });
            //thread.Start();
            //thread.Join();
            //}
            return servCollection;
        }


        //--------------------------------------------------------------------------------
        public async static Task<List<ServicesModel>> GetServices1()
        {
            servCollection = new List<ServicesModel>();
            ServiceController[] scServices = ServiceController.GetServices();
            int counter = 0;
            //lock (servCollection) // Thread safe code
            //{

            //var thread = new Thread(
            //  () =>
            //  {
            return await Task.Run(() =>
            {
                lock (servCollection)
                {
                    foreach (ServiceController scTemp in scServices)
                    {
                        ManagementObject service = new ManagementObject(@"Win32_service.Name='" + scTemp.ServiceName + "'");
                        object o = service.GetPropertyValue("ProcessId");

                        if (servCollection.FindIndex(f => f.ProcessId == ((UInt32)o).ToString()) < 0 && scTemp.Status == ServiceControllerStatus.Running && counter < 15)
                        {
                            ServicesModel temp = new ServicesModel();
                            temp.ServicesName = GetServiceName((int)((UInt32)o));
                            temp.ProcessId = ((UInt32)o).ToString();

                            temp.ServiceState = "Работает";//since it is filtered
                            try
                            {
                                using (ManagementObject service1 = new ManagementObject(new ManagementPath(string.Format("Win32_Service.Name='{0}'", temp.ServicesName))))
                                {
                                    temp.ServiceDescription = service1["Description"].ToString();
                                }
                            }
                            catch (Exception ex) { }
                            temp.ServiceGroup = scTemp.ServicesDependedOn.ToString();

                            servCollection.Add(temp);
                        }
                        counter++;
                    }
                }
              //});
            //thread.Start();
            //thread.Join();
            //}
            return servCollection;
        });
        }

        public static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        public static List<ProcessModel> SortedList(string user, List<ProcessModel> workingList)
        {
            List<ProcessModel> sortedList = new List<ProcessModel>();
            foreach(var elem in workingList)
            {
                if( elem.ProcessUsername_ == user)
                {
                    sortedList.Add(elem);
                }
            }
            return sortedList;
        }

    }
}
