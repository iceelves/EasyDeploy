using CliWrap;
using CliWrap.EventStream;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Management;
using System.Diagnostics;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 命令行交互帮助类
    /// </summary>
    public class CliWrapHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="applicationName">应用名称</param>
        /// <param name="withArguments">参数</param>
        public CliWrapHelper(string workingDirectory, string applicationName, string[] withArguments = null)
        {
            _workingDirectory = workingDirectory;
            _applicationName = applicationName;
            _withArguments = withArguments;
        }

        /// <summary>
        /// 工作目录
        /// </summary>
        private string _workingDirectory { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        private string _applicationName { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        private string[] _withArguments { get; set; }

        /// <summary>
        /// 命令行
        /// </summary>
        private Command _cmd { get; set; }

        /// <summary>
        /// 启动线程 ID
        /// </summary>
        private int _threadID { get; set; }

        public event Action<string> StartedCommandEvent;

        public event Action<string> StandardOutputCommandEvent;

        public event Action<string> StandardErrorCommandEvent;

        public event Action<string> ExitedCommandEvent;

        /// <summary>
        /// 启动
        /// </summary>
        public bool Start()
        {
            if (!string.IsNullOrEmpty(_applicationName))
            {
                _cmd = Cli.Wrap($"{_workingDirectory}{_applicationName}");
                if (!string.IsNullOrEmpty(_workingDirectory))
                {
                    _cmd = _cmd.WithWorkingDirectory(_workingDirectory);
                }
                if (_withArguments != null)
                {
                    _cmd = _cmd.WithArguments(_withArguments);
                }
                Task.Run(async () =>
                {
                    await foreach (var cmdEvent in _cmd.ListenAsync())
                    {
                        switch (cmdEvent)
                        {
                            case StartedCommandEvent started:
                                //Console.WriteLine($"Process started; ID: {started.ProcessId}");
                                _threadID = started.ProcessId;
                                StartedCommandEvent?.Invoke($"{started.ProcessId}");
                                break;
                            case StandardOutputCommandEvent stdOut:
                                //Console.WriteLine($"Out> {stdOut.Text}");
                                StandardOutputCommandEvent?.Invoke($"{stdOut.Text}");
                                break;
                            case StandardErrorCommandEvent stdErr:
                                //Console.WriteLine($"Err> {stdErr.Text}");
                                StandardErrorCommandEvent?.Invoke($"{stdErr.Text}");
                                break;
                            case ExitedCommandEvent exited:
                                //Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                                ExitedCommandEvent?.Invoke($"{exited.ExitCode}");
                                break;
                        }
                    }
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 结束
        /// </summary>
        public void Stop()
        {
            KillProcessAndChildren(_threadID);
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
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

    }
}
