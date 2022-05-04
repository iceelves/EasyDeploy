using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace EasyDeploy.Models
{
    /// <summary>
    /// 服务资源
    /// 程序运行时生成的资源（控制台、Shell）
    /// </summary>
    public class ServiceResourcesModel
    {
        /// <summary>
        /// 服务基础信息
        /// </summary>
        public ServiceModel Service { get; set; }

        /// <summary>
        /// 控制台
        /// </summary>
        public CliWrapHelper CliWrap { get; set; }

        /// <summary>
        /// 终端 Shell
        /// </summary>
        public IceRichTextBox Terminal { get; set; }

        /// <summary>
        /// 计时器每秒刷新
        /// </summary>
        private DispatcherTimer timerPerSecond = null;

        /// <summary>
        /// 进程 PID
        /// </summary>
        public string Pid { get; set; }

        /// <summary>
        /// 监听 Shell
        /// </summary>
        public void MonitorShell()
        {
            if (CliWrap != null && Terminal != null)
            {
                CliWrap.StartedCommandEvent += CliWrap_StartedCommandEvent;
                CliWrap.StandardOutputCommandEvent += CliWrap_StandardOutputCommandEvent;
                CliWrap.StandardErrorCommandEvent += CliWrap_StandardErrorCommandEvent;
                CliWrap.ExitedCommandEvent += CliWrap_ExitedCommandEvent;
            }
        }

        /// <summary>
        /// 启动命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StartedCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText($"Start Service PID:{obj}");
                Pid = obj;
                // 获取到 PID 后启动健康检查
                timerPerSecond = new DispatcherTimer()
                {
                    IsEnabled = true
                };
                timerPerSecond.Interval = TimeSpan.FromSeconds(30);
                timerPerSecond.Tick += TimerPerSecondCallback;
                timerPerSecond?.Start();
            });
        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        /// <param name="state"></param>
        public void TimerPerSecondCallback(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (!PidHelper.GetProcessIsOnline(int.Parse(Pid)))
                {
                    timerPerSecond?.Stop();
                    CliWrap = null;
                    Service.ServiceState = ServiceState.Error;
                    if (Service.AutoReStart)
                    {
                        ReCliWrap();
                    }
                }
            });
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer()
        {
            timerPerSecond?.Stop();
        }

        /// <summary>
        /// 标准输出命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StandardOutputCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }

        /// <summary>
        /// 标准错误命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StandardErrorCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }

        /// <summary>
        /// 退出命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_ExitedCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText($"Stop Service:{obj}");
                timerPerSecond?.Stop();
                CliWrap = null;
                Service.ServiceState = ServiceState.Error;
            });
        }

        /// <summary>
        /// 尝试重启
        /// </summary>
        private void ReCliWrap()
        {
            Service.ServiceState = ServiceState.Wait;
            if (string.IsNullOrEmpty(Service.Parameter))
            {
                CliWrap = new CliWrapHelper(Path.GetDirectoryName(Service.ServicePath), Path.GetFileName(Service.ServicePath));
            }
            else
            {
                CliWrap = new CliWrapHelper(Path.GetDirectoryName(Service.ServicePath), Path.GetFileName(Service.ServicePath), Service.Parameter.Split(' '));
            }
            MonitorShell();
            CliWrap.Start();
            // 通过返回的进程 ID 判断是否运行成功
            Timer timer = new Timer(1000);
            timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
            {
                timer.Enabled = false;
                if (CliWrap.threadID > 0)
                {
                    // 启动成功
                    Service.Pid = $"{CliWrap.threadID}";
                    var vProcessPorts = PidHelper.GetProcessPorts(CliWrap.threadID);
                    if (vProcessPorts != null && vProcessPorts.Count >= 1)
                    {
                        Service.Port = string.Join('/', PidHelper.GetProcessPorts(CliWrap.threadID));
                    }
                    Service.ServiceState = ServiceState.Start;
                }
                else
                {
                    // 启动失败
                    Service.ServiceState = ServiceState.Error;
                    // 等待片刻后重新尝试启动
                    System.Threading.Thread.Sleep(5000);
                    ReCliWrap();
                }
            };
            timer.Enabled = true;
        }
    }
}
