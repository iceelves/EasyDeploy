using EasyDeploy.Helpers;
using EasyDeploy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
                mainMaximization.ButtonImage = Geometry.Parse("M735.786833 62.98859 288.310382 62.98859c-123.567293 0-223.738737 100.164281-223.738737 223.738737l0 447.476451c0 123.555014 100.17042 223.751017 223.738737 223.751017l447.476451 0c123.561154 0 223.738737-100.196003 223.738737-223.751017L959.52557 286.727327C959.52557 163.15287 859.347986 62.98859 735.786833 62.98859zM831.694159 702.260252c0 70.612221-57.238632 127.850853-127.850853 127.850853L320.292793 830.111105c-70.612221 0-127.850853-57.238632-127.850853-127.850853L192.44194 318.708715c0-70.612221 57.238632-127.850853 127.850853-127.850853l383.551536 0c70.612221 0 127.850853 57.238632 127.850853 127.850853L831.695183 702.260252z");
            }
            else
            {
                this.MaxHeight = SystemParameters.WorkArea.Height + 14;
                this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                this.WindowState = WindowState.Maximized;
                mainMaximization.ButtonImage = Geometry.Parse("M146.285714 804.571429h731.428572V365.714286H146.285714v438.857143zM1024 164.571429v694.857142c0 50.285714-41.142857 91.428571-91.428571 91.428572h-841.142858A91.684571 91.684571 0 0 1 0 859.428571v-694.857142C0 114.285714 41.142857 73.142857 91.428571 73.142857h841.142858C982.857143 73.142857 1024 114.285714 1024 164.571429z");
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
            popup_Menu.IsOpen = !popup_Menu.IsOpen;
        }
    }
}
