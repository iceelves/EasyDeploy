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
        static double GetTotalMemoryMBytesCapacity()
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
    }
}
