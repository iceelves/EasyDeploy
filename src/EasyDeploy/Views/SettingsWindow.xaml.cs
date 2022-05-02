using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using EasyDeploy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            // 获取系统配置信息
            // 系统 - 自动启动
            var vSystemConfigInfo_StartWithWindows = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_START_WITH_WINDOWS);
            var vStartWithWindows = !string.IsNullOrEmpty(vSystemConfigInfo_StartWithWindows) && bool.Parse(vSystemConfigInfo_StartWithWindows);
            _initialConfig.Add(SystemConfigHelper.SYSTEM_START_WITH_WINDOWS, vStartWithWindows);
            StartWithWindows.IsChecked = vStartWithWindows;

            // 初始化语言
            Language.ItemsSource = SystemConfigHelper.ListLanguage;
            // 系统 - 语言
            var vSystemConfigInfo_Language = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_LANGUAGE);
            _initialConfig.Add(SystemConfigHelper.SYSTEM_LANGUAGE, vSystemConfigInfo_Language);
            for (int i = 0; i < SystemConfigHelper.ListLanguage.Count; i++)
            {
                if (SystemConfigHelper.ListLanguage[i].FileName.Equals(vSystemConfigInfo_Language))
                {
                    Language.SelectedIndex = i;
                }
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
        /// 记录初始配置
        /// </summary>
        private Dictionary<string, object> _initialConfig = new Dictionary<string, object>();

        /// <summary>
        /// 系统名称
        /// </summary>
        private string _systemName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);

        /// <summary>
        /// 系统进程路径
        /// </summary>
        private string _systemExePath = Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            #region 自动启动
            if (_initialConfig.ContainsKey(SystemConfigHelper.SYSTEM_START_WITH_WINDOWS) && (bool)_initialConfig[SystemConfigHelper.SYSTEM_START_WITH_WINDOWS] != (bool)StartWithWindows.IsChecked)
            {
                if (!WindowsHelper.IsAdministrator())
                {
                    NLogHelper.SaveDebug($"设置开机启动无效：请以管理员方式运行！");
                    IceMessageBox.ShowDialogBox("请以管理员方式运行！");
                }
                else
                {
                    SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_START_WITH_WINDOWS, StartWithWindows.IsChecked.ToString());
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
            }
            #endregion

            #region 语言
            var vSelectedLanguage = Language.SelectedItem as LanguageModel;
            if (_initialConfig.ContainsKey(SystemConfigHelper.SYSTEM_LANGUAGE) && !_initialConfig[SystemConfigHelper.SYSTEM_LANGUAGE].ToString().Equals(vSelectedLanguage.FileName))
            {
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_LANGUAGE, vSelectedLanguage.FileName);
                SystemConfigHelper.SetLanguage(vSelectedLanguage.Resource.Source.OriginalString);
            }
            #endregion

            this.Close();
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
