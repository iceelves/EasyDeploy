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
using System.Windows.Data;
using System.Windows.Controls;

namespace EasyDeploy.ViewModels
{
    public class MainWindowViewModel : BindableBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        /// <summary>
        /// Loaded 事件
        /// </summary>
        public DelegateCommand<Window> LoadedCommand
        {
            get
            {
                return new DelegateCommand<Window>(delegate (Window window)
                {
                    this.window = window;
                    // 窗体关闭前触发事件
                    this.window.Closing += Window_Closing;

                    // 加载系统配置文件
                    // 终端 - 最大行数
                    var vTerminalConfigInfo_MaxRows = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_MAXROWS);
                    TerminalMaxRows = !string.IsNullOrEmpty(vTerminalConfigInfo_MaxRows) && int.Parse(vTerminalConfigInfo_MaxRows) >= 1 ? int.Parse(vTerminalConfigInfo_MaxRows) : 1;
                    // 终端 - 背景颜色
                    var vTerminalConfigInfo_Background = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_BACKGROUND);
                    var vBackground = !string.IsNullOrEmpty(vTerminalConfigInfo_Background) ? vTerminalConfigInfo_Background : "#0C0C0C";
                    TerminalBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vBackground));
                    // 终端 - 文字颜色
                    var vTerminalConfigInfo_Foreground = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FOREGROUND);
                    var vForeground = !string.IsNullOrEmpty(vTerminalConfigInfo_Foreground) ? vTerminalConfigInfo_Foreground : "#FFFFFF";
                    TerminalForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vForeground));
                    // 终端 - 字号
                    var vTerminalConfigInfo_FontSize = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FONTSIZE);
                    TerminalFontSize = !string.IsNullOrEmpty(vTerminalConfigInfo_FontSize) && int.Parse(vTerminalConfigInfo_FontSize) >= 1 ? int.Parse(vTerminalConfigInfo_FontSize) : 1;

                    // 加载日志控件
                    ServicesShell.Add(LogShellGuid, new TabControlTerminalModel()
                    {
                        Header = "Log",
                        Control = CreateBlankRichTextBox()
                    });
                    SetLogLogo();
                    SetLog("Easy Deploy Start!");

                    // 加载 CPU、内存
                    SystemStateHelper systemState = new SystemStateHelper();
                    systemState.CpuCounterChange += SystemState_CpuCounterChange;
                    systemState.RamCounterChange += SystemState_RamCounterChange;

                    // 加载 GPU、显存
                    NvmlStateHelper nvmlState = new NvmlStateHelper();
                    nvmlState.GpuChange += NvmlGpu_GpuChange;
                    nvmlState.MemoryChange += NvmlGpu_MemoryChange;

                    // 加载服务配置文件
                    if (File.Exists(ServiceSavePath))
                    {
                        var vServiceJson = File.ReadAllText(ServiceSavePath);
                        SetLog($"Load Service Config File: {Path.GetFileName(ServiceSavePath)}");
                        Services = JsonConvert.DeserializeObject<ObservableCollection<ServiceModel>>(vServiceJson);
                        SetLog($"Get {Services.Count} Services!");

                        // 启动自动运行项
                        if (Services != null && Services.Count >= 1)
                        {
                            foreach (var item in Services.Where(o => o.AutoStart))
                            {
                                Application.Current?.Dispatcher?.Invoke(() =>
                                {
                                    StartServiceCore(item, true);
                                });
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
        /// 窗体关闭前触发事件
        /// 用于判断关闭所有正在运行的程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ServicesResources != null && ServicesResources.Count >= 1)
            {
                var result = IceMessageBox.ShowDialogBox($"{Application.Current.FindResource("CloseWindowStopServer")}", $"{Application.Current.FindResource("Tips")}", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    StopAllService();
                }
                else
                {
                    // 取消关闭窗体
                    e.Cancel = true;
                }
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
        private string ServiceSavePath = $"{AppDomain.CurrentDomain.BaseDirectory}ServiceConfig.json";

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
        /// CPU 使用率
        /// </summary>
        public string CpuCounter { get; set; } = "0%";

        /// <summary>
        /// 内存计数器
        /// </summary>
        public string RamCounter { get; set; } = "0%";

        /// <summary>
        /// Gpu 使用率
        /// </summary>
        public string GpuCounter { get; set; } = "0%";

        /// <summary>
        /// 显存使用率
        /// </summary>
        public string VRamCounter { get; set; } = "0%";

        /// <summary>
        /// 日志 Shell Guid
        /// </summary>
        public string LogShellGuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 选择服务控制台第几项
        /// </summary>
        public int ServicesShellIndex { get; set; } = 0;

        #region 系统使用率统计
        /// <summary>
        /// CPU 使用率
        /// </summary>
        private void SystemState_CpuCounterChange(double obj)
        {
            CpuCounter = $"{obj:f0}%";
        }

        /// <summary>
        /// 内存使用率
        /// </summary>
        private void SystemState_RamCounterChange(double obj)
        {
            RamCounter = $"{obj:f0}%";
        }

        /// <summary>
        /// GPU 使用率
        /// </summary>
        /// <param name="obj"></param>
        private void NvmlGpu_GpuChange(uint obj)
        {
            GpuCounter = $"{obj}%";
        }

        /// <summary>
        /// 显存使用率
        /// </summary>
        /// <param name="obj"></param>
        private void NvmlGpu_MemoryChange(uint obj)
        {
            VRamCounter = $"{obj}%";
        }
        #endregion

        #region 终端相关配置
        /// <summary>
        /// 最大行数
        /// </summary>
        public int TerminalMaxRows { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public SolidColorBrush TerminalBackground { get; set; }

        /// <summary>
        /// 文字颜色
        /// </summary>
        public SolidColorBrush TerminalForeground { get; set; }

        /// <summary>
        /// 文字大小
        /// </summary>
        public int TerminalFontSize { get; set; }
        #endregion

        /// <summary>
        /// 新增服务
        /// </summary>
        public DelegateCommand AddService
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    AddServiceWindow window = new AddServiceWindow();
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
            SetLog($"Service:{Service.ServiceName} {(IsAuto ? "Auto " : "")}Start");
            if (Service.ServiceState == ServiceState.None)
            {
                Service.ServiceState = ServiceState.Wait;
                // 启动服务
                string strGuid = Guid.NewGuid().ToString();
                Service.Guid = strGuid;
                ServiceResourcesModel serviceResources = new ServiceResourcesModel();
                serviceResources.Service = Service;
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
                int iDetectionNumber = 0;
                Timer timer = new Timer(1000);
                timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
                {
                    timer.Enabled = false;
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        if (serviceResources.CliWrap != null && serviceResources.CliWrap.threadID > 0)
                        {
                            // 启动成功
                            Service.Pid = $"{serviceResources.CliWrap.threadID}";
                            SetLog($"Service:{Service.ServiceName} Start success,PID:{Service.Pid}");
                            var vProcessPorts = PidHelper.GetProcessPorts(serviceResources.CliWrap.threadID);
                            if (vProcessPorts != null && vProcessPorts.Count >= 1)
                            {
                                Service.Port = string.Join('/', vProcessPorts);
                            }
                            // 添加到服务运行时资源列表
                            if (ServicesResources.ContainsKey(strGuid))
                            {
                                ServicesResources[strGuid] = serviceResources;
                            }
                            else
                            {
                                ServicesResources.Add(strGuid, serviceResources);
                            }
                            // 添加到服务控制台绑定控件
                            ServicesShell.Add(strGuid, new TabControlTerminalModel() { Header = Service.ServiceName, Control = serviceResources.Terminal });
                            Service.ServiceState = ServiceState.Start;
                        }
                        else
                        {
                            if (iDetectionNumber++ < 20)
                            {
                                // 不确认是否启动成功，循环重试
                                timer.Enabled = true;
                                SetLog($"Service:{Service.ServiceName} Start uncertain success,Try again {iDetectionNumber}/10 times");
                            }
                            else
                            {
                                // 启动失败
                                Service.ServiceState = ServiceState.Error;
                            }
                        }
                    });
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
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        StartServiceCore(Service, false);
                    });
                });
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="Service">服务信息</param>
        private void StopServiceCore(ServiceModel Service)
        {
            SetLog($"Service:{Service.ServiceName} Stop");
            if ((Service.ServiceState == ServiceState.Start || Service.ServiceState == ServiceState.Error) && !string.IsNullOrEmpty(Service.Guid))
            {
                Service.ServiceState = ServiceState.Wait;
                if (!string.IsNullOrEmpty(Service.Pid))
                {
                    // 关闭服务
                    PidHelper.KillProcessAndChildren(int.Parse(Service.Pid));
                    // 移除运行时资源
                    if (ServicesResources.ContainsKey(Service.Guid))
                    {
                        ServicesResources[Service.Guid].StopTimer();
                        ServicesResources.Remove(Service.Guid);
                    }
                    if (ServicesShell.ContainsKey(Service.Guid))
                    {
                        ServicesShell[Service.Guid].Control.Collect();
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
                    // 等待两秒后再次检查是否获取到 PID，还是获取不到的话移除运行时资源
                    Timer timer = new Timer(2000);
                    timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
                    {
                        timer.Enabled = false;
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            if (!string.IsNullOrEmpty(Service.Pid))
                            {
                                PidHelper.KillProcessAndChildren(int.Parse(Service.Pid));
                            }
                            // 移除运行时资源
                            if (ServicesResources.ContainsKey(Service.Guid))
                            {
                                ServicesResources[Service.Guid].StopTimer();
                                ServicesResources.Remove(Service.Guid);
                            }
                            if (ServicesShell.ContainsKey(Service.Guid))
                            {
                                ServicesShell[Service.Guid].Control.Collect();
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
            else
            {
                // Guid 都获取不到时恢复到默认状态
                Service.ServiceState = ServiceState.None;
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
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        StopServiceCore(Service);
                    });
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
                    if (Service != null && !string.IsNullOrEmpty(Service.Guid) &&
                        ServicesResources.ContainsKey(Service.Guid) &&
                        ServicesShell.ContainsKey(Service.Guid))
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
                    AddServiceWindow window = new AddServiceWindow(Service);
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
                        IceMessageBox.ShowDialogBox($"{Application.Current.FindResource("DeleteServerStopServer")}", $"{Application.Current.FindResource("Tips")}");
                    }
                    else
                    {
                        var result = IceMessageBox.ShowDialogBox($"{Application.Current.FindResource("ConfirmDeleteService")}", $"{Application.Current.FindResource("Tips")}", MessageBoxButton.OKCancel);
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
        /// 设置
        /// </summary>
        public DelegateCommand Settings
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    SettingsWindow settingsWindow = new SettingsWindow();
                    settingsWindow.ShowDialog();
                    if (settingsWindow.OutConfig != null && settingsWindow.OutConfig.Count >= 1)
                    {
                        // 终端 - 最大行数
                        if (settingsWindow.OutConfig.ContainsKey(SystemConfigHelper.TERMINAL_MAXROWS))
                        {
                            var vTerminalMaxRows = $"{settingsWindow.OutConfig[SystemConfigHelper.TERMINAL_MAXROWS]}";
                            var vMaxRows = !string.IsNullOrEmpty(vTerminalMaxRows) && int.Parse(vTerminalMaxRows) >= 1 ? int.Parse(vTerminalMaxRows) : 1;
                            TerminalMaxRows = vMaxRows;
                        }

                        // 终端 - 背景颜色
                        if (settingsWindow.OutConfig.ContainsKey(SystemConfigHelper.TERMINAL_BACKGROUND))
                        {
                            var vTerminalBackground = $"{settingsWindow.OutConfig[SystemConfigHelper.TERMINAL_BACKGROUND]}";
                            var vBackground = !string.IsNullOrEmpty(vTerminalBackground) ? vTerminalBackground : "#0C0C0C";
                            TerminalBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vBackground));
                        }

                        // 终端 - 文字颜色
                        if (settingsWindow.OutConfig.ContainsKey(SystemConfigHelper.TERMINAL_FOREGROUND))
                        {
                            var vTerminalForeground = $"{settingsWindow.OutConfig[SystemConfigHelper.TERMINAL_FOREGROUND]}";
                            var vForeground = !string.IsNullOrEmpty(vTerminalForeground) ? vTerminalForeground : "#FFFFFF";
                            TerminalForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vForeground));
                        }

                        // 终端 - 字号
                        if (settingsWindow.OutConfig.ContainsKey(SystemConfigHelper.TERMINAL_FONTSIZE))
                        {
                            var vTerminalFontSize = $"{settingsWindow.OutConfig[SystemConfigHelper.TERMINAL_FONTSIZE]}";
                            var vFontSize = !string.IsNullOrEmpty(vTerminalFontSize) && int.Parse(vTerminalFontSize) >= 1 ? int.Parse(vTerminalFontSize) : 1;
                            TerminalFontSize = vFontSize;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 关于
        /// </summary>
        public DelegateCommand About
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    AboutWindow aboutWindow = new AboutWindow();
                    aboutWindow.ShowDialog();
                });
            }
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        public DelegateCommand ShowWindow
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    if (this.window.WindowState == WindowState.Minimized)
                    {
                        this.window.WindowState = WindowState.Normal;
                    }
                    this.window?.Show();
                    this.window?.Activate();
                });
            }
        }

        /// <summary>
        /// 显示或隐藏窗体
        /// </summary>
        public DelegateCommand ShowOrHide
        {
            get
            {
                return new DelegateCommand(delegate ()
                {
                    if (this.window.IsVisible)
                    {
                        this.window?.Hide();
                    }
                    else
                    {
                        this.window?.Show();
                        this.window?.Activate();
                    }
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
                SetLog($"App shutdown,Stop all running services");
                foreach (var item in ServicesResources)
                {
                    PidHelper.KillProcessAndChildren(item.Value?.CliWrap?.threadID);
                    SetLog($"App shutdown,Stop services:{item.Value?.Service?.ServiceName},PID:{item.Value?.Service?.Pid}");
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
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                // 创建控件
                vRichText = new IceRichTextBox();
                vRichText.SetBinding(IceRichTextBox.MaxRowsProperty, new Binding("TerminalMaxRows") { Source = this });
                vRichText.SetBinding(IceRichTextBox.TerminalBackgroundProperty, new Binding("TerminalBackground") { Source = this });
                vRichText.SetBinding(IceRichTextBox.TerminalForegroundProperty, new Binding("TerminalForeground") { Source = this });
                vRichText.SetBinding(IceRichTextBox.TerminalFontSizeProperty, new Binding("TerminalFontSize") { Source = this });

                // 创建后处理
                vRichText.Init();
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
                if (e.Delta < 0)
                {
                    if (TerminalFontSize >= 6)
                    {
                        // 字号最小为 5
                        TerminalFontSize -= 1;
                    }
                }
                else
                {
                    if (TerminalFontSize <= 31)
                    {
                        // 字号最大为 32
                        TerminalFontSize += 1;
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
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ServicesShell[LogShellGuid]?.Control?.SetText($"\u001b[90m{DateTime.Now}: \u001b[0m{log}");
                });
            }

            // 保存信息日志
            NLogHelper.SaveInfo(log);
        }

        /// <summary>
        /// 写入 LOGO
        /// </summary>
        private void SetLogLogo()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                int l1 = 96;
                int l2 = 93;
                int l3 = 92;
                int l4 = 95;
                int r1 = 96;
                int r2 = 93;
                int r3 = 92;
                int r4 = 91;
                int r5 = 96;
                int r6 = 93;
                // Easy Deploy
                ServicesShell[LogShellGuid]?.Control?.SetText($"");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$$$$$$$\\ \u001b[{l2}m        \u001b[{l3}m           \u001b[{l4}m              \u001b[{r1}m$$$$$$$\\  \u001b[{r2}m          \u001b[{r3}m          \u001b[{r4}m$$\\ \u001b[{r5}m          \u001b[{r6}m          ");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$  _____|\u001b[{l2}m        \u001b[{l3}m           \u001b[{l4}m              \u001b[{r1}m$$  __$$\\ \u001b[{r2}m          \u001b[{r3}m          \u001b[{r4}m$$ |\u001b[{r5}m          \u001b[{r6}m          ");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$ |      \u001b[{l2}m$$$$$$\\ \u001b[{l3}m  $$$$$$$\\ \u001b[{l4}m$$\\   $$\\     \u001b[{r1}m$$ |  $$ |\u001b[{r2}m $$$$$$\\  \u001b[{r3}m $$$$$$\\  \u001b[{r4}m$$ |\u001b[{r5}m $$$$$$\\  \u001b[{r6}m$$\\   $$\\ ");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$$$$\\    \u001b[{l2}m\\____$$\\ \u001b[{l3}m$$  _____|\u001b[{l4}m$$ |  $$ |    \u001b[{r1}m$$ |  $$ |\u001b[{r2}m$$  __$$\\ \u001b[{r3}m$$  __$$\\ \u001b[{r4}m$$ |\u001b[{r5}m$$  __$$\\ \u001b[{r6}m$$ |  $$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$  __|   \u001b[{l2}m$$$$$$$ |\u001b[{l3}m\\$$$$$$\\  \u001b[{l4}m$$ |  $$ |    \u001b[{r1}m$$ |  $$ |\u001b[{r2}m$$$$$$$$ |\u001b[{r3}m$$ /  $$ |\u001b[{r4}m$$ |\u001b[{r5}m$$ /  $$ |\u001b[{r6}m$$ |  $$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$ |     \u001b[{l2}m$$  __$$ |\u001b[{l3}m \\____$$\\ \u001b[{l4}m$$ |  $$ |    \u001b[{r1}m$$ |  $$ |\u001b[{r2}m$$   ____|\u001b[{r3}m$$ |  $$ |\u001b[{r4}m$$ |\u001b[{r5}m$$ |  $$ |\u001b[{r6}m$$ |  $$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$$$$$$$ \u001b[{l2}m\\$$$$$$$ |\u001b[{l3}m$$$$$$$  |\u001b[{l4}m\\$$$$$$$ |    \u001b[{r1}m$$$$$$$  |\u001b[{r2}m\\$$$$$$$\\ \u001b[{r3}m$$$$$$$  |\u001b[{r4}m$$ |\u001b[{r5}m\\$$$$$$  |\u001b[{r6}m\\$$$$$$$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m\\________|\u001b[{l2}m\\_______|\u001b[{l3}m\\_______/ \u001b[{l4}m \\____$$ |    \u001b[{r1}m\\_______/ \u001b[{r2}m \\_______|\u001b[{r3}m$$  ____/ \u001b[{r4}m\\__|\u001b[{r5}m \\______/ \u001b[{r6}m \\____$$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m          \u001b[{l2}m         \u001b[{l3}m          \u001b[{l4}m$$\\   $$ |    \u001b[{r1}m          \u001b[{r2}m          \u001b[{r3}m$$ |      \u001b[{r4}m    \u001b[{r5}m          \u001b[{r6}m$$\\   $$ |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m          \u001b[{l2}m         \u001b[{l3}m          \u001b[{l4}m\\$$$$$$  |    \u001b[{r1}m          \u001b[{r2}m          \u001b[{r3}m$$ |      \u001b[{r4}m    \u001b[{r5}m          \u001b[{r6}m\\$$$$$$  |");
                ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m          \u001b[{l2}m         \u001b[{l3}m          \u001b[{l4}m \\______/     \u001b[{r1}m          \u001b[{r2}m          \u001b[{r3}m\\__|      \u001b[{r4}m    \u001b[{r5}m          \u001b[{r6}m \\______/ ");

                // Ice Elves
                //ServicesShell[LogShellGuid]?.Control?.SetText($"");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$$$$$\\ \u001b[{l2}m         \u001b[{l3}m              \u001b[{r1}m$$$$$$$$\\ \u001b[{r2}m$$\\ \u001b[{r3}m           \u001b[{r4}m          \u001b[{l4}m          ");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m\\_$$  _|\u001b[{l2}m         \u001b[{l3}m              \u001b[{r1}m$$  _____|\u001b[{r2}m$$ |\u001b[{r3}m           \u001b[{r4}m          \u001b[{l4}m          ");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m  $$ |  \u001b[{l2}m$$$$$$$\\ \u001b[{l3}m $$$$$$\\      \u001b[{r1}m$$ |      \u001b[{r2}m$$ |\u001b[{r3}m$$\\    $$\\ \u001b[{r4}m $$$$$$\\  \u001b[{l4}m $$$$$$$\\ ");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m  $$ | \u001b[{l2}m$$  _____|\u001b[{l3}m$$  __$$\\     \u001b[{r1}m$$$$$\\    \u001b[{r2}m$$ |\u001b[{r3}m\\$$\\  $$  |\u001b[{r4}m$$  __$$\\ \u001b[{l4}m$$  _____|");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m  $$ | \u001b[{l2}m$$ /      \u001b[{l3}m$$$$$$$$ |    \u001b[{r1}m$$  __|   \u001b[{r2}m$$ |\u001b[{r3}m \\$$\\$$  / \u001b[{r4}m$$$$$$$$ |\u001b[{l4}m\\$$$$$$\\  ");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m  $$ | \u001b[{l2}m$$ |      \u001b[{l3}m$$   ____|    \u001b[{r1}m$$ |      \u001b[{r2}m$$ |\u001b[{r3}m  \\$$$  /  \u001b[{r4}m$$   ____|\u001b[{l4}m \\____$$\\ ");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m$$$$$$ \u001b[{l2}m\\$$$$$$$\\ \u001b[{l3}m\\$$$$$$$\\     \u001b[{r1}m$$$$$$$$\\ \u001b[{r2}m$$ |\u001b[{r3}m   \\$  /   \u001b[{r4}m\\$$$$$$$\\ \u001b[{l4}m$$$$$$$  |");
                //ServicesShell[LogShellGuid]?.Control?.SetText($"  \u001b[{l1}m\\______|\u001b[{l2}m\\_______|\u001b[{l3}m \\_______|    \u001b[{r1}m\\________|\u001b[{r2}m\\__|\u001b[{r3}m    \\_/    \u001b[{r4}m \\_______|\u001b[{l4}m\\_______/ ");

                ServicesShell[LogShellGuid]?.Control?.SetText($"");
            });
        }
    }
}
