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
    /// 系统占用率统计
    /// </summary>
    public class SystemStateHelper
    {
        /// <summary>
        /// 获取全局占用率
        /// </summary>
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

                    var ramAvailable = RamCounter.NextValue();
                    var memUsage = Math.Round((TotalMemoryMBytesCapacity - ramAvailable) / TotalMemoryMBytesCapacity, 4) * 100;
                    memUsage = memUsage >= 100 ? 100 : memUsage;

                    CpuCounterChange?.Invoke(cpuUsage);
                    RamCounterChange?.Invoke(memUsage);
                    Thread.Sleep(500);
                }
            });
        }

        /// <summary>
        /// 获取指定 pid 进程占用率
        /// </summary>
        /// <param name="pid">pid</param>
        public SystemStateHelper(int pid)
        {
            Task.Run(() =>
            {
                const float mega = 1024 * 1024;
                var vInstanceName = GetProcessInstanceName(pid);

                if (!string.IsNullOrEmpty(vInstanceName))
                {
                    PerformanceCounter cpuPerformanceCounter = new PerformanceCounter("Process", "% Processor Time", vInstanceName);
                    PerformanceCounter memoryPerformanceCounter = new PerformanceCounter("Process", "Working Set - Private", vInstanceName);

                    while (true)
                    {
                        try
                        {
                            float mainCpu = cpuPerformanceCounter.NextValue() / Environment.ProcessorCount;
                            mainCpu = mainCpu >= 100 ? 100 : mainCpu;

                            float mainRam = memoryPerformanceCounter.NextValue() / mega;

                            CpuCounterChange.Invoke(mainCpu);
                            RamCounterChange.Invoke(mainRam);
                        }
                        catch (Exception)
                        {
                            // pid 查询不到进程
                        }

                        Thread.Sleep(500);
                    }
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
