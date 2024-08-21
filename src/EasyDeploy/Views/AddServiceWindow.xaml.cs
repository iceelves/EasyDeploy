using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using EasyDeploy.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyDeploy.Views
{
    /// <summary>
    /// AddService.xaml 的交互逻辑
    /// </summary>
    public partial class AddServiceWindow : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AddServiceWindow(ServiceModel serviceModel = null)
        {
            InitializeComponent();

            this.TitleBar.MouseDown += TitleBar_MouseDown;

            if (serviceModel != null)
            {
                this.Title = $"{Application.Current.FindResource("EditService")}";

                ServiceModel = serviceModel;
                ServiceName.Text = serviceModel.ServiceName;
                ServicePath.Text = serviceModel.ServicePath;
                if (!string.IsNullOrEmpty(serviceModel.ServicePath))
                {
                    IsAbsolutePath = PathHelper.IsAbsolutePath(serviceModel.ServicePath);
                    if (IsAbsolutePath)
                    {
                        AbsolutePath.IsChecked = true;
                    }
                    else
                    {
                        RelativePath.IsChecked = true;
                    }
                }
                Parameter.Text = serviceModel.Parameter;
                AutoStart.IsChecked = serviceModel.AutoStart;
                AutoReStart.IsChecked = serviceModel.AutoReStart;
            }
        }

        /// <summary>
        /// 拖动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 是否为绝对路径
        /// 默认为绝对路径
        /// </summary>
        private bool IsAbsolutePath { get; set; } = true;

        /// <summary>
        /// 选择路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".exe";
            dialog.Filter = "EXE File|*.exe;*.bat|全部文件|*.*";
            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    ServicePath.Text = IsAbsolutePath ? dialog.FileName : PathHelper.AbsoluteToRelative(dialog.FileName);
                }
                catch (Exception ex)
                {
                    IceMessageBox.ShowDialogBox(ex.Message);
                }
            }
        }

        /// <summary>
        /// 全局服务配置
        /// </summary>
        public ServiceModel ServiceModel { get; set; }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 检查数据
            if (string.IsNullOrEmpty(ServiceName.Text))
            {
                BorderFlashing(ServiceName);
                return;
            }
            if (string.IsNullOrEmpty(ServicePath.Text))
            {
                BorderFlashing(ServicePath);
                return;
            }
            // 保存数据
            if (ServiceModel == null)
            {
                ServiceModel = new ServiceModel();
            }
            ServiceModel.ServiceName = ServiceName.Text;
            ServiceModel.ServicePath = ServicePath.Text;
            ServiceModel.Parameter = Parameter.Text;
            ServiceModel.AutoStart = (bool)AutoStart.IsChecked;
            ServiceModel.AutoReStart = (bool)AutoReStart.IsChecked;
            this.Close();
        }

        /// <summary>
        /// 最小化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 文本框边框闪烁
        /// 用于错误提示显示
        /// </summary>
        /// <param name="containingObject"></param>
        private void BorderFlashing(FrameworkElement containingObject)
        {
            if (this.TryFindResource("BlinkAnime") is Storyboard blinkStoryboard)
            {
                blinkStoryboard.Begin(containingObject, true);

                Timer timer = new Timer(5000);
                timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
                {
                    timer.Enabled = false;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        blinkStoryboard.Stop(containingObject);
                    }));
                };
                timer.Enabled = true;
            }
        }

        /// <summary>
        /// 切换路径类型时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PathType_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.AbsolutePath.IsChecked)
            {
                IsAbsolutePath = true;
                if (!string.IsNullOrEmpty(ServicePath.Text))
                {
                    ServicePath.Text = PathHelper.RelativeToAbsolute(ServicePath.Text, AppDomain.CurrentDomain.BaseDirectory);
                }
            }
            else
            {
                IsAbsolutePath = false;
                if (!string.IsNullOrEmpty(ServicePath.Text))
                {
                    try
                    {
                        ServicePath.Text = PathHelper.AbsoluteToRelative(ServicePath.Text, AppDomain.CurrentDomain.BaseDirectory);
                    }
                    catch (Exception ex)
                    {
                        IceMessageBox.ShowDialogBox(ex.Message);
                    }
                }
            }
        }
    }
}
