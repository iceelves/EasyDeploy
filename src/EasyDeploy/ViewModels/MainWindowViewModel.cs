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
using System.Timers;
using System.Windows;

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
                    // 加载配置文件
                    if (File.Exists(ServiceSavePath))
                    {
                        var vServiceJson = File.ReadAllText(ServiceSavePath);
                        Services = JsonConvert.DeserializeObject<ObservableCollection<ServiceModel>>(vServiceJson);
                    }
                    else
                    {
                        // TODO:未查到配置文件
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

        #region 用户控件
        private IceRichTextBox _iceRichTextBox;
        /// <summary>
        /// 界面富文本输出窗口
        /// </summary>
        public IceRichTextBox IceRichTextBox
        {
            get
            {
                if (_iceRichTextBox == null)
                {
                    _iceRichTextBox = window.FindName("richText") as IceRichTextBox;
                }
                return _iceRichTextBox;
            }
        }
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
                    AddService window = new AddService();
                    window.ShowDialog();
                    var vServiceModel = window.ServiceModel;
                    if (vServiceModel != null)
                    {
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
        public DelegateCommand<ServiceModel> StartService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    if (Service.ServiceState == ServiceState.None || Service.ServiceState == ServiceState.Error)
                    {
                        Service.ServiceState = ServiceState.Start;
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
                        serviceResources.CliWrap.Start();
                        // 通过返回的进程 ID 判断是否运行成功
                        Timer timer = new Timer(2000);
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
                                ServicesResources.Add(strGuid, serviceResources);
                            }
                            else
                            {
                                // 启动失败
                                Service.ServiceState = ServiceState.Error;
                            }
                        };
                        timer.Enabled = true;
                    }
                });
            }
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public DelegateCommand<ServiceModel> StopService
        {
            get
            {
                return new DelegateCommand<ServiceModel>(delegate (ServiceModel Service)
                {
                    if (Service.ServiceState == ServiceState.Start)
                    {
                        Service.ServiceState = ServiceState.None;
                        // 关闭服务
                        PidHelper.KillProcessAndChildren(int.Parse(Service.Pid));
                        if (ServicesResources.ContainsKey(Service.Guid))
                        {
                            ServicesResources.Remove(Service.Guid);
                        }
                        Service.Pid = null;
                        Service.Port = null;
                        Service.Guid = null;
                    }
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
                            Services.Remove(Service);
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
        public void StopAllService()
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
    }
}
