using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using EasyDeploy.Models;
using EasyDeploy.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace EasyDeploy.ViewModels
{
    public class MainWindowViewModel : BindableBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        /// <summary>
        /// Load事件
        /// </summary>
        public DelegateCommand<Window> LoadedCommand
        {
            get
            {
                return new DelegateCommand<Window>(delegate (Window window)
                {
                    this.window = window;

                    // 加载日志控件
                    ServicesShell.Add(LogShellGuid, new TabControlTerminalModel()
                    {
                        Header = "Log",
                        Control = CreateBlankRichTextBox()
                    });
                    SetLog("Easy Deploy Start!");

                    // 加载配置文件
                    if (File.Exists(ServiceSavePath))
                    {
                        var vServiceJson = File.ReadAllText(ServiceSavePath);
                        SetLog($"Load Config File: {ServiceSavePath}");
                        Services = JsonConvert.DeserializeObject<ObservableCollection<ServiceModel>>(vServiceJson);
                        SetLog($"Get {Services.Count} Services!");

                        // 启动自动运行项
                        if (Services != null && Services.Count >= 1)
                        {
                            foreach (var item in Services.Where(o => o.AutoStart))
                            {
                                StartServiceCore(item, true);
                            }
                        }
                    }
                    else
                    {
                        // 未查到配置文件
                        SetLog("Not Found Config File!");
                    }
                });
            }
        }

        /// <summary>
        /// OnLoaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessExit(object sender, EventArgs e)
        {
            StopAllService();
        }

        /// <summary>
        /// Window 窗体
        /// </summary>
        public Window window { get; private set; }

        /// <summary>
        /// 服务配置文件保存路径
        /// </summary>
        private string ServiceSavePath = "ServiceConfig.json";

        /// <summary>
        /// 服务信息集合
        /// </summary>
        public ObservableCollection<ServiceModel> Services { get; set; } = new ObservableCollection<ServiceModel>();

        /// <summary>
        /// 服务运行时资源
        /// </summary>
        public ObservableDictionary<string, ServiceResourcesModel> ServicesResources { get; set; } = new ObservableDictionary<string, ServiceResourcesModel>();

        /// <summary>
        /// 服务控制台绑定控件
        /// </summary>
        public ObservableDictionary<string, TabControlTerminalModel> ServicesShell { get; set; } = new ObservableDictionary<string, TabControlTerminalModel>();

        /// <summary>
        /// 日志 Shell Guid
        /// </summary>
        public string LogShellGuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 选择服务控制台第几项
        /// </summary>
        public int ServicesShellIndex { get; set; } = 0;

        /// <summary>
        /// 新增服务
        /// </summary>
        public DelegateCommand AddService
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    AddService window = new AddService();
                    window.ShowDialog();
                    var vServiceModel = window.ServiceModel;
                    if (vServiceModel != null)
                    {
                        SetLog($"Add Service: {vServiceModel.ServiceName}");
                        // 填充数据
                        Services.Add(vServiceModel);
                        // 保存数据集
                        var vServiceJson = JsonConvert.SerializeObject(Services, Formatting.Indented);
                        StreamWriter sw = new StreamWriter(ServiceSavePath);
                        sw.WriteLine(vServiceJson);
                        sw.Close();
                    }
                });
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="Service">服务信息</param>
        /// <param name="IsAuto">是否自动触发</param>
        private void StartServiceCore(ServiceModel Service, bool IsAuto)
        {
            SetLog($"{(IsAuto ? "Auto " : "")}Start Service: {Service.ServiceName}");
            if (Service.ServiceState == ServiceState.None || Service.ServiceState == ServiceState.Error)
            {
                Service.ServiceState = ServiceState.Wait;
                // 启动服务
                string strGuid = Guid.NewGuid().ToString();
                Service.Guid = strGuid;
                ServiceResourcesModel serviceResources = new ServiceResourcesModel();
                if (string.IsNullOrEmpty(Service.Parameter))
                {
                    serviceResources.CliWrap = new CliWrapHelper(Path.GetDirectoryName(Service.ServicePath), Path.GetFileName(Service.ServicePath));
                }
                else
                {
                    serviceResources.CliWrap = new CliWrapHelper(Path.GetDirectoryName(Service.ServicePath), Path.GetFileName(Service.ServicePath), Service.Parameter.Split(' '));
                }
                // 加载终端 Shell
                serviceResources.Terminal = CreateBlankRichTextBox();
                serviceResources.MonitorShell();
                serviceResources.CliWrap.Start();
                // 通过返回的进程 ID 判断是否运行成功
                Timer timer = new Timer(1000);
                timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
                {
                    timer.Enabled = false;
                    if (serviceResources.CliWrap.threadID > 0)
                    {
                        // 启动成功
                        Service.Pid = $"{serviceResources.CliWrap.threadID}";
                        var vProcessPorts = PidHelper.GetProcessPorts(serviceResources.CliWrap.threadID);
                        if (vProcessPorts != null && vProcessPorts.Count >= 1)
                        {
                            Service.Port = string.Join('/', PidHelper.GetProcessPorts(serviceResources.CliWrap.threadID));
                        }
                        // 添加到服务运行时资源列表
                        ServicesResources.Add(strGuid, serviceResources);
                        // 添加到服务控制台绑定控件
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ServicesShell.Add(strGuid, new TabControlTerminalModel() { Header = Service.ServiceName, Control = serviceResources.Terminal });
                        });
                        Service.ServiceState = ServiceState.Start;
                    }
                    else
                    {
                        // 启动失败
                        Service.ServiceState = ServiceState.Error;
                    }
                };
                timer.Enabled = true;
            }
        }

        /// <summary>
        /// 启动服务 按键触发
        /// </summary>
        public DelegateCommand<ServiceModel> StartService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    StartServiceCore(Service, false);
                });
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="Service">服务信息</param>
        private void StopServiceCore(ServiceModel Service)
        {
            SetLog($"Stop Service: {Service.ServiceName}");
            if (Service.ServiceState == ServiceState.Start && !string.IsNullOrEmpty(Service.Guid))
            {
                Service.ServiceState = ServiceState.Wait;
                if (!string.IsNullOrEmpty(Service.Pid))
                {
                    // 关闭服务
                    PidHelper.KillProcessAndChildren(int.Parse(Service.Pid));
                    // 移除运行时资源
                    if (ServicesResources.ContainsKey(Service.Guid))
                    {
                        ServicesResources.Remove(Service.Guid);
                    }
                    if (ServicesShell.ContainsKey(Service.Guid))
                    {
                        ServicesShell.Remove(Service.Guid);
                    }
                    Service.Pid = null;
                    Service.Port = null;
                    Service.Guid = null;
                    Service.ServiceState = ServiceState.None;
                }
                else
                {
                    // 未成功启动
                    // 等待两秒后再次检查是否获取到 PID,以 PID结束进程或
                    Timer timer = new Timer(2000);
                    timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
                    {
                        timer.Enabled = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!string.IsNullOrEmpty(Service.Pid))
                            {
                                PidHelper.KillProcessAndChildren(int.Parse(Service.Pid));
                            }
                            // 移除运行时资源
                            if (ServicesResources.ContainsKey(Service.Guid))
                            {
                                ServicesResources.Remove(Service.Guid);
                            }
                            if (ServicesShell.ContainsKey(Service.Guid))
                            {
                                ServicesShell.Remove(Service.Guid);
                            }
                            Service.Pid = null;
                            Service.Port = null;
                            Service.Guid = null;
                            Service.ServiceState = ServiceState.None;
                        });
                    };
                    timer.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public DelegateCommand<ServiceModel> StopService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    StopServiceCore(Service);
                });
            }
        }

        /// <summary>
        /// 浏览服务文件夹
        /// </summary>
        public DelegateCommand<ServiceModel> BrowseService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    if (!string.IsNullOrEmpty(Service.ServicePath))
                    {
                        var vPath = PathHelper.IsAbsolutePath(Service.ServicePath) ? Service.ServicePath : PathHelper.RelativeToAbsolute(Service.ServicePath);
                        var vDirectory = Path.GetDirectoryName(vPath);
                        if (Directory.Exists(vDirectory))
                        {
                            SetLog($"Open Directory: {vDirectory}");
                            System.Diagnostics.Process.Start("explorer.exe", vDirectory);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 打开服务输出窗口
        /// </summary>
        public DelegateCommand<ServiceModel> ShellService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    SetLog($"Open Shell: {Service.ServiceName}");
                    if (Service != null && !string.IsNullOrEmpty(Service.Guid))
                    {
                        if (ServicesResources.ContainsKey(Service.Guid))
                        {
                            if (ServicesShell.ContainsKey(Service.Guid))
                            {
                                // 切换分页
                                for (int i = 0; i < ServicesShell.Count; i++)
                                {
                                    if (ServicesShell.ElementAt(i).Key.Equals(Service.Guid))
                                    {
                                        ServicesShellIndex = i;
                                        // 滚动条切至底部
                                        ServicesShell.ElementAt(i).Value.Control.ScrollToEnd();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 编辑服务
        /// </summary>
        public DelegateCommand<ServiceModel> EditService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    SetLog($"Edge Service: {Service.ServiceName}");
                    AddService window = new AddService(Service);
                    window.ShowDialog();
                    Service = window.ServiceModel;
                    // 保存数据集
                    var vServiceJson = JsonConvert.SerializeObject(Services, Formatting.Indented);
                    StreamWriter sw = new StreamWriter(ServiceSavePath);
                    sw.WriteLine(vServiceJson);
                    sw.Close();
                });
            }
        }

        /// <summary>
        /// 删除服务
        /// </summary>
        public DelegateCommand<ServiceModel> DeleteService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    if (Service.ServiceState == ServiceState.Start)
                    {
                        IceMessageBox.ShowDialogBox($"Close Service Before Delete : {Service.ServiceName} !");
                    }
                    else
                    {
                        MessageBoxResult result = IceMessageBox.ShowDialogBox($"Confirm Delete of Service: {Service.ServiceName} ?", "Tips", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            SetLog($"Remove Service: {Service.ServiceName}");
                            Services.Remove(Service);
                            // 保存数据集
                            var vServiceJson = JsonConvert.SerializeObject(Services, Formatting.Indented);
                            StreamWriter sw = new StreamWriter(ServiceSavePath);
                            sw.WriteLine(vServiceJson);
                            sw.Close();
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        public DelegateCommand Close
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    if (ServicesResources != null && ServicesResources.Count >= 1)
                    {
                        MessageBoxResult result = IceMessageBox.ShowDialogBox($"There are services that have not been closed. Do you want to close them?", "Tips", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            StopAllService();
                        }
                        else
                        {
                            return;
                        }
                    }
                    this.window.Close();
                });
            }
        }

        /// <summary>
        /// 关闭所有服务
        /// </summary>
        private void StopAllService()
        {
            if (ServicesResources != null && ServicesResources.Count >= 1)
            {
                // 停止所有正在运行的服务
                foreach (var item in ServicesResources)
                {
                    PidHelper.KillProcessAndChildren(item.Value.CliWrap.threadID);
                }
                ServicesResources = null;
                // 清空关联数据
                foreach (var item in Services)
                {
                    item.ServiceState = ServiceState.None;
                    item.Pid = null;
                    item.Port = null;
                    item.Guid = null;
                }
            }
        }

        /// <summary>
        /// 创建空白富文本控件
        /// </summary>
        /// <returns></returns>
        private IceRichTextBox CreateBlankRichTextBox()
        {
            IceRichTextBox vRichText = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                vRichText = new IceRichTextBox()
                {
                    IsReadOnly = true,
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C0C0C")),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontFamily = new FontFamily("Cascadia Mono"),
                    MaxRows = 5000
                };
                vRichText.ClearText();
                vRichText.PreviewMouseWheel += VRichText_PreviewMouseWheel;
            });
            return vRichText;
        }

        /// <summary>
        /// 富文本框关联 Ctrl + 鼠标滑轮缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VRichText_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var vControl = sender as IceRichTextBox;
                if (e.Delta < 0)
                {
                    if (vControl.FontSize >= 6)
                    {
                        // 字号最小为 5
                        vControl.FontSize -= 1;
                    }
                }
                else
                {
                    if (vControl.FontSize <= 31)
                    {
                        // 字号最大为 32
                        vControl.FontSize += 1;
                    }
                }
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="log"></param>
        private void SetLog(string log)
        {
            if (ServicesShell != null && ServicesShell.Count >= 1 && ServicesShell.ContainsKey(LogShellGuid))
            {
                ServicesShell[LogShellGuid]?.Control?.SetText($"\u001b[90m{DateTime.Now}: \u001b[0m{log}");
            }
        }
    }
}
