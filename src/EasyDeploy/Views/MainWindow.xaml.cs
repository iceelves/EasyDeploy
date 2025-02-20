using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 初始化系统语言
            SystemConfigHelper.SetLanguage();

#if DEBUG
            // 运行控制台程序
            ConsoleWindow.Show();
#endif

            // 设置对齐方式
            SetAlignment();
        }

        /// <summary>
        /// 下拉菜单按钮是否按下
        /// </summary>
        private bool _isMoreMenusDown = false;

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 最大化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maximization_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
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
        /// 下拉菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreMenus_Click(object sender, RoutedEventArgs e)
        {
            // 为了避免点击按钮反复打开 Popup
            // 通过按钮是否按下作为判断条件
            if (_isMoreMenusDown)
            {
                _isMoreMenusDown = false;
                popup_Menu.IsOpen = !popup_Menu.IsOpen;
            }
        }

        /// <summary>
        /// 下拉菜单按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreMenus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMoreMenusDown = true;
        }

        /// <summary>
        /// 设置对齐方式
        /// 设置为惯用左手 菜单出现在手的右侧
        /// </summary>
        public static void SetAlignment()
        {
            // 获取系统是以Left-handed（true）还是Right-handed（false）
            var ifLeft = SystemParameters.MenuDropAlignment;

            if (ifLeft)
            {
                // change to false
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field?.SetValue(null, false);
            }
        }
    }
}
