using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using EasyDeploy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        /// Loaded
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
            SystemLanguage.ItemsSource = SystemConfigHelper.ListLanguage;
            // 系统 - 语言
            var vSystemConfigInfo_Language = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_LANGUAGE);
            _initialConfig.Add(SystemConfigHelper.SYSTEM_LANGUAGE, vSystemConfigInfo_Language);
            for (int i = 0; i < SystemConfigHelper.ListLanguage.Count; i++)
            {
                if (SystemConfigHelper.ListLanguage[i].FileName.Equals(vSystemConfigInfo_Language))
                {
                    SystemLanguage.SelectedIndex = i;
                }
            }

            // 获取应用配置
            // 应用 - 启动等待次数
            var vApplicationConfigInfo_StartWaitTimes = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_APPLICATION, SystemConfigHelper.APPLICATION_START_WAIT_TIMES);
            _initialConfig.Add(SystemConfigHelper.APPLICATION_START_WAIT_TIMES, vApplicationConfigInfo_StartWaitTimes);
            StartWaitTimes.Text = $"{vApplicationConfigInfo_StartWaitTimes}";

            // 获取终端配置
            // 终端 - 最大行数
            var vTerminalConfigInfo_MaxRows = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_MAXROWS);
            var vMaxRows = !string.IsNullOrEmpty(vTerminalConfigInfo_MaxRows) && int.Parse(vTerminalConfigInfo_MaxRows) >= 1 ? int.Parse(vTerminalConfigInfo_MaxRows) : 1;
            _initialConfig.Add(SystemConfigHelper.TERMINAL_MAXROWS, vMaxRows);
            TerminalMaxRows.Text = $"{vMaxRows}";

            // 终端 - 字号
            var vTerminalConfigInfo_FontSize = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FONTSIZE);
            var vFontSize = !string.IsNullOrEmpty(vTerminalConfigInfo_FontSize) && int.Parse(vTerminalConfigInfo_FontSize) >= 1 ? int.Parse(vTerminalConfigInfo_FontSize) : 1;
            _initialConfig.Add(SystemConfigHelper.TERMINAL_FONTSIZE, vFontSize);
            TerminalFontSize.Text = $"{vFontSize}";

            // 终端 - 背景颜色
            var vTerminalConfigInfo_Background = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_BACKGROUND);
            var vBackground = !string.IsNullOrEmpty(vTerminalConfigInfo_Background) ? vTerminalConfigInfo_Background : "#0C0C0C";
            _initialConfig.Add(SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
            TerminalBackground.SelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vBackground));

            // 终端 - 文字颜色
            var vTerminalConfigInfo_Foreground = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FOREGROUND);
            var vForeground = !string.IsNullOrEmpty(vTerminalConfigInfo_Foreground) ? vTerminalConfigInfo_Foreground : "#FFFFFF";
            _initialConfig.Add(SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
            TerminalForeground.SelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vForeground));
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
        /// 返回配置
        /// </summary>
        public Dictionary<string, object> OutConfig = new Dictionary<string, object>();

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
            #region 系统设置
            #region 自动启动
            if (_initialConfig.ContainsKey(SystemConfigHelper.SYSTEM_START_WITH_WINDOWS) && (bool)_initialConfig[SystemConfigHelper.SYSTEM_START_WITH_WINDOWS] != (bool)StartWithWindows.IsChecked)
            {
                if (!WindowsHelper.IsAdministrator())
                {
                    NLogHelper.SaveDebug($"设置开机启动无效：请以管理员方式运行！");
                    IceMessageBox.ShowDialogBox($"{Application.Current.FindResource("RunAsAdministrator")}", $"{Application.Current.FindResource("Tips")}");
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
                            NLogHelper.SaveDebug($"设置 Windows 开机启动{(vStartup ? "成功" : "失败")}");
                        }
                    }
                    else
                    {
                        //移除开机自启
                        if (vDicAllStartupItems.ContainsKey(_systemName))
                        {
                            var vStartup = RegistryHelper.DeleteStartupItems(_systemName);
                            NLogHelper.SaveDebug($"移除 Windows 开机启动{(vStartup ? "成功" : "失败")}");
                        }
                    }
                }
            }
            #endregion

            #region 语言
            if (SystemLanguage.SelectedItem is LanguageModel selectedLanguage)
            {
                if (_initialConfig.ContainsKey(SystemConfigHelper.SYSTEM_LANGUAGE) && !$"{_initialConfig[SystemConfigHelper.SYSTEM_LANGUAGE]}".Equals(selectedLanguage.FileName))
                {
                    SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_SYSTEM, SystemConfigHelper.SYSTEM_LANGUAGE, selectedLanguage.FileName);
                    SystemConfigHelper.SetLanguage(selectedLanguage.Resource.Source.OriginalString);
                }
            }
            #endregion
            #endregion

            #region 应用设置
            #region 启动等待次数
            StartWaitTimes.Text = StartWaitTimes.Text.Replace(" ", "");
            if (_initialConfig.ContainsKey(SystemConfigHelper.APPLICATION_START_WAIT_TIMES) && !_initialConfig[SystemConfigHelper.APPLICATION_START_WAIT_TIMES].ToString().Equals(StartWaitTimes.Text))
            {
                if (StartWaitTimes.Text.Length > 10 || double.Parse(StartWaitTimes.Text) < 1 || double.Parse(StartWaitTimes.Text) > int.MaxValue)
                {
                    BorderFlashing(StartWaitTimes);
                    return;
                }
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_APPLICATION, SystemConfigHelper.APPLICATION_START_WAIT_TIMES, StartWaitTimes.Text);
                OutConfig.Add(SystemConfigHelper.APPLICATION_START_WAIT_TIMES, StartWaitTimes.Text);
            }
            #endregion
            #endregion

            #region 终端设置
            #region 最大行数
            TerminalMaxRows.Text = TerminalMaxRows.Text.Replace(" ", "");
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_MAXROWS) && !_initialConfig[SystemConfigHelper.TERMINAL_MAXROWS].ToString().Equals(TerminalMaxRows.Text))
            {
                if (TerminalMaxRows.Text.Length > 10 || double.Parse(TerminalMaxRows.Text) < 10 || double.Parse(TerminalMaxRows.Text) > int.MaxValue)
                {
                    BorderFlashing(TerminalMaxRows);
                    return;
                }
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_MAXROWS, TerminalMaxRows.Text);
                OutConfig.Add(SystemConfigHelper.TERMINAL_MAXROWS, TerminalMaxRows.Text);
            }
            #endregion

            #region 文字大小
            TerminalFontSize.Text = TerminalFontSize.Text.Replace(" ", "");
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_FONTSIZE) && !_initialConfig[SystemConfigHelper.TERMINAL_FONTSIZE].ToString().Equals(TerminalFontSize.Text))
            {
                if (TerminalFontSize.Text.Length > 10 || double.Parse(TerminalFontSize.Text) < 5 || double.Parse(TerminalFontSize.Text) > 32)
                {
                    BorderFlashing(TerminalFontSize);
                    return;
                }
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FONTSIZE, TerminalFontSize.Text);
                OutConfig.Add(SystemConfigHelper.TERMINAL_FONTSIZE, TerminalFontSize.Text);
            }
            #endregion

            #region 背景颜色
            var vBackground = TerminalBackground.SelectColor.Color.ToString();
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_BACKGROUND) && !_initialConfig[SystemConfigHelper.TERMINAL_BACKGROUND].ToString().Equals(vBackground))
            {
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
                OutConfig.Add(SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
            }
            #endregion

            #region 文字颜色
            var vForeground = TerminalForeground.SelectColor.Color.ToString();
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_FOREGROUND) && !_initialConfig[SystemConfigHelper.TERMINAL_FOREGROUND].ToString().Equals(vForeground))
            {
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
                OutConfig.Add(SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
            }
            #endregion
            #endregion

            this.Close();
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
        /// 限制文本框只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
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
    }
}
