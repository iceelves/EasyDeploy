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

            // 获取终端配置
            // 终端 - 最大行数
            var vTerminalConfigInfo_MaxRows = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_MAXROWS);
            var vMaxRows = !string.IsNullOrEmpty(vTerminalConfigInfo_MaxRows) && int.Parse(vTerminalConfigInfo_MaxRows) >= 1 ? int.Parse(vTerminalConfigInfo_MaxRows) : 1;
            _initialConfig.Add(SystemConfigHelper.TERMINAL_MAXROWS, vMaxRows);
            MaxRows.Text = $"{vMaxRows}";

            // 终端 - 背景颜色
            var vTerminalConfigInfo_Background = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_BACKGROUND);
            var vBackground = !string.IsNullOrEmpty(vTerminalConfigInfo_Background) ? vTerminalConfigInfo_Background : "#0C0C0C";
            _initialConfig.Add(SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
            Background.SelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vBackground));

            // 终端 - 文字颜色
            var vTerminalConfigInfo_Foreground = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FOREGROUND);
            var vForeground = !string.IsNullOrEmpty(vTerminalConfigInfo_Foreground) ? vTerminalConfigInfo_Foreground : "#FFFFFF";
            _initialConfig.Add(SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
            Foreground.SelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(vForeground));

            // 终端 - 字号
            var vTerminalConfigInfo_FontSize = SystemConfigHelper.GetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FONTSIZE);
            var vFontSize = !string.IsNullOrEmpty(vTerminalConfigInfo_FontSize) && int.Parse(vTerminalConfigInfo_FontSize) >= 1 ? int.Parse(vTerminalConfigInfo_FontSize) : 1;
            _initialConfig.Add(SystemConfigHelper.TERMINAL_FONTSIZE, vFontSize);
            FontSize.Text = $"{vFontSize}";
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
            #endregion

            #region 终端设置
            #region 最大行数
            MaxRows.Text = MaxRows.Text.Replace(" ", "");
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_MAXROWS) && !_initialConfig[SystemConfigHelper.TERMINAL_MAXROWS].ToString().Equals(MaxRows.Text))
            {
                if (MaxRows.Text.Length > 10 || double.Parse(MaxRows.Text) < 10 || double.Parse(MaxRows.Text) > int.MaxValue)
                {
                    BorderFlashing(MaxRows);
                    return;
                }
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_MAXROWS, MaxRows.Text);
                OutConfig.Add(SystemConfigHelper.TERMINAL_MAXROWS, MaxRows.Text);
            }
            #endregion

            #region 背景颜色
            var vBackground = Background.SelectColor.Color.ToString();
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_BACKGROUND) && !_initialConfig[SystemConfigHelper.TERMINAL_BACKGROUND].ToString().Equals(vBackground))
            {
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
                OutConfig.Add(SystemConfigHelper.TERMINAL_BACKGROUND, vBackground);
            }
            #endregion

            #region 文字颜色
            var vForeground = Foreground.SelectColor.Color.ToString();
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_FOREGROUND) && !_initialConfig[SystemConfigHelper.TERMINAL_FOREGROUND].ToString().Equals(vForeground))
            {
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
                OutConfig.Add(SystemConfigHelper.TERMINAL_FOREGROUND, vForeground);
            }
            #endregion

            #region 文字大小
            FontSize.Text = FontSize.Text.Replace(" ", "");
            if (_initialConfig.ContainsKey(SystemConfigHelper.TERMINAL_FONTSIZE) && !_initialConfig[SystemConfigHelper.TERMINAL_FONTSIZE].ToString().Equals(FontSize.Text))
            {
                if (FontSize.Text.Length > 10 || double.Parse(FontSize.Text) < 1 || double.Parse(FontSize.Text) > 32)
                {
                    BorderFlashing(FontSize);
                    return;
                }
                SystemConfigHelper.SetSystemConfigInfo(SystemConfigHelper.SECTION_TERMINAL, SystemConfigHelper.TERMINAL_FONTSIZE, FontSize.Text);
                OutConfig.Add(SystemConfigHelper.TERMINAL_FONTSIZE, FontSize.Text);
            }
            #endregion
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
            Storyboard _blinkStoryboard = this.TryFindResource("BlinkAnime") as Storyboard;
            _blinkStoryboard.Begin(containingObject, true);

            Timer timer = new Timer(5000);
            timer.Elapsed += delegate (object senderTimer, ElapsedEventArgs eTimer)
            {
                timer.Enabled = false;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    _blinkStoryboard.Stop(containingObject);
                }));
            };
            timer.Enabled = true;
        }
    }
}
