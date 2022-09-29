using EasyDeploy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 系统使用率统计
    /// </summary>
    public class SystemStateHelper
    {
        public SystemStateHelper()
        {
            Task.Run(() =>
            {
                PerformanceCounter CpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
                PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");
                double TotalMemoryMBytesCapacity = GetTotalMemoryMBytesCapacity();

                while (true)
                {
                    var cpuUsage = CpuCounter.NextValue();
                    cpuUsage = cpuUsage >= 100 ? 100 : cpuUsage;
                    cpuUsage = cpuUsage <= 0 ? 0 : cpuUsage;

                    var ramAvailable = RamCounter.NextValue();
                    var memUsage = Math.Round((TotalMemoryMBytesCapacity - ramAvailable) / TotalMemoryMBytesCapacity, 4) * 100;
                    memUsage = memUsage >= 100 ? 100 : memUsage;
                    memUsage = memUsage <= 0 ? 0 : memUsage;

                    CpuCounterChange?.Invoke(cpuUsage);
                    RamCounterChange?.Invoke(memUsage);
                    Thread.Sleep(500);
                }
            });
        }

        public SystemStateHelper(int pid)
        {
            Task.Run(() =>
            {
                const float mega = 1024 * 1024;

                while (true)
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}");
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        var vPid = int.Parse($"{mo["ProcessID"]}");
                        var vInstanceName = GetProcessInstanceName(vPid);
                        if (!string.IsNullOrEmpty(vInstanceName))
                        {
                            PerformanceCounter cpuPerformanceCounter = new PerformanceCounter("Process", "% Processor Time", vInstanceName);
                            PerformanceCounter memoryPerformanceCounter = new PerformanceCounter("Process", "Working Set - Private", vInstanceName);

                            float mainCpu = cpuPerformanceCounter.NextValue() / Environment.ProcessorCount;
                            mainCpu = mainCpu >= 100 ? 100 : mainCpu;
                            mainCpu = mainCpu <= 0 ? 0 : mainCpu;

                            float mainRam = memoryPerformanceCounter.NextValue() / mega;
                            mainRam = mainRam >= 100 ? 100 : mainRam;
                            mainRam = mainRam <= 0 ? 0 : mainRam;

                            Console.WriteLine($"CPU:{mainCpu:f2}%\tMemory:{mainRam:f2}M");

                            if (CpuArmChangs.ContainsKey(vPid))
                            {
                                CpuArmChangs[vPid].CpuChang = mainCpu;
                                CpuArmChangs[vPid].RamChang = mainRam;
                            }
                            else
                            {
                                CpuArmChangs.Add(vPid, new SystemStateModel()
                                {
                                    PID = vPid,
                                    CpuChang = mainCpu,
                                    RamChang = mainRam,
                                });
                            }
                        }
                        else
                        {
                            if (CpuArmChangs.ContainsKey(vPid))
                            {
                                CpuArmChangs.Remove(vPid);
                            }
                        }
                    }
                    SubprocessCpuArmCountersChange.Invoke(CpuArmChangs);
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// CPU 使用率
        /// </summary>
        public event Action<double> CpuCounterChange;

        /// <summary>
        /// 内存使用率
        /// </summary>
        public event Action<double> RamCounterChange;

        /// <summary>
        /// 子进程 CPU、内存 使用率
        /// </summary>
        public event Action<Dictionary<int, SystemStateModel>> SubprocessCpuArmCountersChange;

        /// <summary>
        /// 子程序 CPU 使用率集合
        /// </summary>
        public Dictionary<int, SystemStateModel> CpuArmChangs = new Dictionary<int, SystemStateModel>();

        /// <summary>
        /// 获取总内存字节容量
        /// </summary>
        /// <returns></returns>
        private double GetTotalMemoryMBytesCapacity()
        {
            using (var mc = new ManagementClass("Win32_PhysicalMemory"))
            {
                using (var moc = mc.GetInstances())
                {
                    double totalCapacity = 0d;
                    foreach (var mo in moc)
                    {
                        var moCapacity = long.Parse(mo.Properties["Capacity"].Value.ToString());
                        totalCapacity += Math.Round(moCapacity / 1024.0 / 1024, 1);
                    }
                    return totalCapacity;
                }
            }
        }

        /// <summary>
        /// 获取进程实例名称
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProcessInstanceName(int pid)
        {
            PerformanceCounterCategory processCategory = new PerformanceCounterCategory("Process");
            string[] runnedInstances = processCategory.GetInstanceNames();

            foreach (string runnedInstance in runnedInstances)
            {
                using (PerformanceCounter performanceCounter = new PerformanceCounter("Process", "ID Process", runnedInstance, true))
                {
                    try
                    {
                        if ((int)performanceCounter?.RawValue == pid)
                        {
                            return runnedInstance;
                        }
                    }
                    catch (Exception)
                    { }
                }
            }
            return "";
        }
    }
}
