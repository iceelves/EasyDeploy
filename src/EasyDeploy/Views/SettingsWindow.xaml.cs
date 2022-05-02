using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyDeploy.Views
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.TitleBar.MouseDown += TitleBar_MouseDown;
            this.Loaded += SettingsWindow_Loaded;
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //获取系统配置信息
            var vSystemConfigInfo_RememberPassword = SystemConfigHelper.GetSystemConfigInfo("System", "StartWithWindows");
            StartWithWindows.IsChecked = !string.IsNullOrEmpty(vSystemConfigInfo_RememberPassword) && bool.Parse(vSystemConfigInfo_RememberPassword);
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
        /// 系统名称
        /// </summary>
        private string _systemName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);

        /// <summary>
        /// 系统进程路径
        /// </summary>
        private string _systemExePath = Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// 开机启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartWithWindows_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.IsAdministrator())
            {
                NLogHelper.SaveDebug($"设置开机启动无效：请以管理员方式运行！");
                IceMessageBox.ShowDialogBox("请以管理员方式运行！");
                return;
            }
            SystemConfigHelper.SetSystemConfigInfo("System", "StartWithWindows", StartWithWindows.IsChecked.ToString());
            var vDicAllStartupItems = RegistryHelper.GetAllStartupItems();
            if ((bool)StartWithWindows.IsChecked)
            {
                //设置开机自启
                if (!vDicAllStartupItems.ContainsKey(_systemName))
                {
                    var vStartup = RegistryHelper.CreateStartupItems(_systemName, _systemExePath);
                    NLogHelper.SaveDebug($"设置Windows开机启动{(vStartup ? "成功" : "失败")}");
                }
            }
            else
            {
                //移除开机自启
                if (vDicAllStartupItems.ContainsKey(_systemName))
                {
                    var vStartup = RegistryHelper.DeleteStartupItems(_systemName);
                    NLogHelper.SaveDebug($"移除Windows开机启动{(vStartup ? "成功" : "失败")}");
                }
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {

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
