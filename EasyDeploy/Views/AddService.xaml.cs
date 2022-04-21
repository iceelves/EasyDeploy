using EasyDeploy.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyDeploy.Views
{
    /// <summary>
    /// AddService.xaml 的交互逻辑
    /// </summary>
    public partial class AddService : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AddService(ServiceModel serviceModel = null)
        {
            InitializeComponent();
            this.TitleBar.MouseDown += TitleBar_MouseDown;

            if (serviceModel != null)
            {
                ServiceModel = serviceModel;
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
        /// 选择路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".exe";
            dialog.Filter = "EXE File|*.exe|全部文件|*.*";
            if ((bool)dialog.ShowDialog())
            {
                ServicePath.Text = dialog.FileName;
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
            if (ServiceModel == null)
            {
                ServiceModel = new ServiceModel();
            }
            ServiceModel.ServiceName = ServiceName.Text;
            ServiceModel.ServicePath = ServicePath.Text;
            ServiceModel.Parameter = Parameter.Text;
            ServiceModel.AutoStart = AutoStart.IsEnabled;
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
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
