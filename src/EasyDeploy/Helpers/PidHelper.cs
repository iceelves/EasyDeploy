using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 进程 ID 相关帮助类
    /// </summary>
    public static class PidHelper
    {
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        public static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
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

        /// <summary>
        /// 获取进程是否在线
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static bool GetProcessIsOnline(int pid)
        {
            if (pid == 0)
            {
                return false;
            }
            foreach (var item in GetAllProcess())
            {
                if (item.Key.Equals(pid))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有进程信息
        /// </summary>
        public static Dictionary<int, string> GetAllProcess()
        {
            Dictionary<int, string> dicAllProcess = new Dictionary<int, string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process");
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                dicAllProcess.Add(int.Parse($"{mo["ProcessID"]}"), $"{mo["Name"]}");
            }
            return dicAllProcess;
        }

        /// <summary>
        /// 获取机器端口占用情况
        /// </summary>
        /// <returns>字典(pid,端口号)</returns>
        public static Dictionary<int, List<int>> GetProcessPorts()
        {
            Dictionary<int, List<int>> portinfo = null;

            try
            {
                portinfo = new Dictionary<int, List<int>>();
                Process pro = new Process();
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();
                pro.StandardInput.WriteLine("netstat -ano");
                pro.StandardInput.WriteLine("exit");
                Regex reg = new Regex("\\s+", RegexOptions.Compiled);
                string line = null;
                while ((line = pro.StandardOutput.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                    {
                        line = reg.Replace(line, ",");
                        string[] arr = line.Split(',');
                        string soc = arr[1];
                        int pos = soc.LastIndexOf(':');
                        int pot = int.Parse(soc.Substring(pos + 1));
                        int pid = int.Parse(arr[4]);
                        if (portinfo.ContainsKey(pid))
                        {
                            if (!portinfo[pid].Contains(pot))
                            {
                                portinfo[pid].Add(pot);
                            }
                        }
                        else
                        {
                            List<int> ls = new List<int>();
                            ls.Add(pot);
                            portinfo.Add(pid, ls);
                        }

                    }
                    else if (line.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
                    {
                        line = reg.Replace(line, ",");
                        string[] arr = line.Split(',');
                        string soc = arr[1];
                        int pos = soc.LastIndexOf(':');
                        int pot = int.Parse(soc.Substring(pos + 1));
                        int pid = int.Parse(arr[3]);
                        if (portinfo.ContainsKey(pid))
                        {
                            if (!portinfo[pid].Contains(pot))
                            {
                                portinfo[pid].Add(pot);
                            }
                        }
                        else
                        {
                            List<int> ls = new List<int>();
                            ls.Add(pot);
                            portinfo.Add(pid, ls);
                        }
                    }
                }
                pro.Close();
            }
            catch (Exception)
            { }
            return portinfo;
        }

        /// <summary>
        /// 获取机器端口占用情况
        /// </summary>
        /// <param name="pid">进程ID</param>
        /// <returns>字典(pid,端口号)</returns>
        public static List<int> GetProcessPorts(int pid)
        {
            var vProcessPorts = GetProcessPorts();
            return vProcessPorts.ContainsKey(pid) ? vProcessPorts[pid] : null;
        }
    }
}
